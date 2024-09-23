using Common.Engine;
using Common.Engine.Config;
using Common.Engine.Notifications;
using Common.Engine.Surveys;
using Common.Engine.Surveys.Model;
using Entities.DB.Entities;
using Entities.DB.Entities.AuditLog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Text.Json;
using Web.Bots.Cards;
using Web.Bots.Dialogues.Abstract;

namespace Web.Bots.Dialogues;

/// <summary>
/// Entrypoint to all new conversations
/// </summary>
public class SurveyDialogue : StoppableDialogue
{
    private readonly ILogger<SurveyDialogue> _tracer;
    private readonly BotActionsHelper _botActionsHelper;
    private readonly UserState _userState;
    private readonly IConversationResumeHandler _conversationResumeHandler;
    const string CACHE_NAME_NEXT_COPILOT_ACTION_TO_SURVEY = "NextCopilotAction";
    const string CACHE_NAME_CUSTOM_SURVEY_PAGE_NUMBER = "SurveyPageNumber";
    const string CACHE_NAME_SURVEY_ID = "SurveyId";
    const string BTN_SEND_SURVEY = "Go on then";

    /// <summary>
    /// Setup dialogue flow
    /// </summary>
    public SurveyDialogue(StopBotheringMeDialogue stopBotheringMeDialogue, BotConfig configuration, BotConversationCache botConversationCache, ILogger<SurveyDialogue> tracer,
        BotActionsHelper botActionsHelper,
        UserState userState, IServiceProvider services, GraphServiceClient graphServiceClient, IConversationResumeHandler conversationResumeHandler)
        : base(nameof(SurveyDialogue), botConversationCache, configuration, services, graphServiceClient)
    {
        _tracer = tracer;
        _botActionsHelper = botActionsHelper;
        _userState = userState;
        _conversationResumeHandler = conversationResumeHandler;
        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(stopBotheringMeDialogue);

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog),
        [
            NewChat,
            SendSurveyOrNot,
            ProcessInitialSurveyResponse,
            ProcessFollowUp
        ]));
        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        InitialDialogId = nameof(WaterfallDialog);
    }


    /// <summary>
    /// Main entry-point for bot new chat. User is either responding to the intro card or has said something to the bot.
    /// </summary>
    private async Task<DialogTurnResult> NewChat(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Check if user wants to stop the bot from bothering them
        var cancel = await base.InterruptAsync(stepContext, cancellationToken);
        if (cancel != null)
            return cancel;

        // Get cached user and conversation. We do it here too in case the conversation cache has been cleared.
        var botUser = await BotUserUtils.GetBotUserAsync(stepContext.Context, _botConfig, _graphServiceClient);
        var cachedUserAndConversation = await base.GetCachedUser(botUser);
        if (cachedUserAndConversation == null || cachedUserAndConversation.UserPrincipalName == null)
        {
            // This may be the first time we've met this user if they've spoken to the bot, but the conversation cache has been cleared.
            // Add them to the cache and try again.
            await base._botConversationCache.AddConversationReferenceToCache(stepContext.Context.Activity, botUser);
            cachedUserAndConversation = await base.GetCachedUser(botUser);

            // Now we've tried to add their user to the conversation cache, check again
            if (cachedUserAndConversation == null || cachedUserAndConversation.UserPrincipalName == null)
            {
                // Should never happen. End the conversation.
                await SendMsg(stepContext.Context!,
                    "For some reason I can't find your login name in our chat history, which is very odd. Are you a guest? " +
                    "It looks like I can't do much without this so, bye!");
                return await stepContext.EndDialogAsync();
            }
        }

        var prevActionText = stepContext.Context?.Activity?.Text;

        // Figure out why we're here
        var surveyInitialResponse = GetSurveyInitialResponseFrom(stepContext.Context?.Activity?.Text);
        if (surveyInitialResponse != null)
        {
            // User has responded to the initial survey card probably from the bot new thread event, so we can skip the intro card
            return await stepContext.NextAsync(surveyInitialResponse);
        }

        if (prevActionText == "{}" || prevActionText == "Start Survey")              // In teams, this is empty JSon for some reason, unlike in Bot Framework SDK client
        {
            // User has started the dialogue with "start survey", probably from the intro card on bot 1st join a new thread.
            return await stepContext.NextAsync(new FoundChoice() { Value = BTN_SEND_SURVEY });
        }
        else
        {
            // We're here because the user said something to the bot outside this dialogue flow, and it wasn't a response to the intro card.
            // Probably because they said "sup" or something to us, randomly
            await SendMsg(stepContext.Context!, "Oh heeeyyy, you're back! I assume you want another chance to rate copilot if you're talking to me...");

            var nextActionAndResumeCard = await _conversationResumeHandler.GetProactiveConversationResumeConversationCard(cachedUserAndConversation.UserPrincipalName);
            return await PromptWithCard(stepContext, nextActionAndResumeCard.Item2);
        }
    }

    /// <summary>
    /// User has chosen to fill out a survey or not
    /// </summary>
    private async Task<DialogTurnResult> SendSurveyOrNot(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        if (stepContext.Result is SurveyInitialResponse surveyInitialResponse)
        {
            // User has responded to the initial survey card outside of the dialogue (bot new thread event probably).
            return await stepContext.NextAsync(surveyInitialResponse);
        }
        else if (stepContext.Result is string resultString)
        {
            // Response survey sent by previous step
            var suveyResponse = GetSurveyInitialResponseFrom(resultString);
            if (suveyResponse != null)
            {
                return await stepContext.NextAsync(stepContext.Result);
            }
        }
        else if (stepContext.Result is FoundChoice response)
        {
            if (response.Value == BTN_SEND_SURVEY)
            {
                var botUser = await BotUserUtils.GetBotUserAsync(stepContext.Context, _botConfig, _graphServiceClient);
                var chatUserAndConversation = await base.GetCachedUser(botUser);
                if (chatUserAndConversation == null || chatUserAndConversation.UserPrincipalName == null)
                {
                    // We really shouldn't get here, but just in case...
                    await SendMsg(stepContext.Context, "Oops, can't send you a survey if I don't know who you are. Sorry about that...");
                    return await stepContext.EndDialogAsync();
                }

                var (nextCopilotEvent, surveyCard) = await _conversationResumeHandler.GetProactiveConversationResumeConversationCard(chatUserAndConversation.UserPrincipalName);

                if (nextCopilotEvent != null)
                {
                    // Remember copilot action user is being surveyed for
                    await _userState.CreateProperty<BaseCopilotSpecificEvent>(CACHE_NAME_NEXT_COPILOT_ACTION_TO_SURVEY).SetAsync(stepContext.Context, nextCopilotEvent);
                }

                return await PromptWithCard(stepContext, surveyCard);
            }
            else
            {
                await SendMsg(stepContext.Context, "No worries. Ping me if you change your mind later.");
                return await stepContext.EndDialogAsync();
            }
        }

        // We're here because we don't know what the user is responding to
        await SendMsg(stepContext.Context, "Oops, I wasn't expecting that response. Let's pretend this didn't happen...");
        return await stepContext.EndDialogAsync();

    }

    /// <summary>
    /// User responds to initial survey card
    /// </summary>
    private async Task<DialogTurnResult> ProcessInitialSurveyResponse(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var result = JsonSerializer.Deserialize<SurveyInitialResponse>(stepContext.Context.Activity.Text);

        // Get selected survey, if there is one
        var userPropNextCopilotEvent = _userState.CreateProperty<CopilotChat?>(CACHE_NAME_NEXT_COPILOT_ACTION_TO_SURVEY);
        var surveyedEvent = await userPropNextCopilotEvent.GetAsync(stepContext.Context, () => null);

        // Process response
        if (result != null)
        {
            var parsedResponse = result.Response;
            if (parsedResponse == null)
            {
                await SendMsg(stepContext.Context, "Oops, I got a survey response back I can't understand. Sorry about that...");
                return await stepContext.EndDialogAsync();
            }

            // Respond if valid
            if (parsedResponse.ScoreGiven > 0)
            {
                // Save survey data?
                var botUser = await BotUserUtils.GetBotUserAsync(stepContext.Context, _botConfig, _graphServiceClient);
                SurveyPage? surveyPage = null;
                var chatUserAndConvo = await base.GetCachedUser(botUser);
                if (chatUserAndConvo == null || chatUserAndConvo.UserPrincipalName == null)
                    await SendMsg(stepContext.Context, "Oops, can't report feedback for anonymous users. Your login is needed even if we don't report on it. Thanks for letting me know anyway.");
                else
                {
                    // Update survey data using the survey manager
                    var surveyIdUpdatedOrCreated = 0;
                    await base.GetSurveyManagerService(async surveyManager =>
                    {
                        // Start custom surveys
                        surveyPage = await surveyManager.GetSurveyPage(0);
                        if (surveyPage != null)
                        {
                            surveyPage.SurveyPageCommonAdaptiveCardTemplateJson = Utils.ReadResource(BotConstants.SurveyCustomPageCommon);
                        }

                        if (surveyedEvent != null)
                        {
                            // Log survey result for specific copilot event
                            try
                            {
                                surveyIdUpdatedOrCreated = await surveyManager.Loader.UpdateSurveyResultWithInitialScore(surveyedEvent.AuditEvent, parsedResponse.ScoreGiven);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                _tracer.LogWarning($"Survey record doesn't exist for event {surveyedEvent.AuditEvent.Id} to update with score.");
                                // Survey record doesn't exist for this event, for some reason. Ignore and end the conversation.
                            }
                            await userPropNextCopilotEvent.DeleteAsync(stepContext.Context);
                        }
                        else
                        {
                            // Log survey result for general survey
                            surveyIdUpdatedOrCreated = await surveyManager.Loader.LogDisconnectedSurveyResult(parsedResponse.ScoreGiven, chatUserAndConvo.UserPrincipalName);
                        }
                    });

                    // Continue diag logic 
                    if (surveyIdUpdatedOrCreated == 0)
                    {
                        await SendMsg(stepContext.Context, "Oops, I can't find the event you're responding to. Sorry about that...");
                        return await stepContext.EndDialogAsync();
                    }
                    else
                    {
                        // Remember the survey ID for follow-up questions
                        await _userState.CreateProperty<int>(CACHE_NAME_SURVEY_ID).SetAsync(stepContext.Context, surveyIdUpdatedOrCreated);
                    }
                }

                // Success!
                // Cycle through survey pages, starting at 0
                await _userState.CreateProperty<int>(CACHE_NAME_CUSTOM_SURVEY_PAGE_NUMBER).SetAsync(stepContext.Context, 0);

                // Send reaction card to initial survey with follow-up form for more details
                var responseCard = new BotReactionCard(parsedResponse.ReplyToFeedback, parsedResponse.IsHappy, surveyPage);
                return await PromptWithCard(stepContext, responseCard);
            }
        }

        // If we're here, the survey response was invalid for one reason or another
        await SendMsg(stepContext.Context, "Oops, I got a survey response back I can't understand. Sorry about that...");
        return await stepContext.EndDialogAsync();
    }

    private async Task<DialogTurnResult> ProcessFollowUp(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var surveyIdUpdatedOrCreated = await _userState.CreateProperty<int>(CACHE_NAME_SURVEY_ID).GetAsync(stepContext.Context);

        var result = stepContext.Context.Activity.Text;
        var botUser = await BotUserUtils.GetBotUserAsync(stepContext.Context, _botConfig, _graphServiceClient);

        var chatUserAndConvo = await base.GetCachedUser(botUser);

        if (chatUserAndConvo?.UserPrincipalName != null)
        {
            var surveyResponse = new SurveyPageUserResponse(result, chatUserAndConvo.UserPrincipalName);

            if (surveyIdUpdatedOrCreated > 0 && surveyResponse.IsValid)
            {
                var lastPageId = await _userState.CreateProperty<int>(CACHE_NAME_CUSTOM_SURVEY_PAGE_NUMBER).GetAsync(stepContext.Context);
                var goAroundAgain = false;
                await base.GetSurveyManagerService(async surveyManager =>
                {
                    // Add follow-up survey data
                    await surveyManager.SaveCustomSurveyResponse(surveyResponse);

                    // Get next survey page
                    var surveyPage = await surveyManager.GetSurveyPage(lastPageId + 1);
                    if (surveyPage != null)
                    {
                        surveyPage.SurveyPageCommonAdaptiveCardTemplateJson = Utils.ReadResource(BotConstants.SurveyCustomPageCommon);
                        await _userState.CreateProperty<int>(CACHE_NAME_CUSTOM_SURVEY_PAGE_NUMBER).SetAsync(stepContext.Context, lastPageId + 1);

                        // Send follow-up survey page
                        var followUpCard = new CustomSurveyPageCard(surveyPage);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(followUpCard.GetCardAttachment()), cancellationToken);
                        goAroundAgain = true;
                    }
                    else
                    {
                        // Sign off & say thanks
                        var allDoneCard = new BotDiagFinishedCard().GetCardAttachment();
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(allDoneCard), cancellationToken);
                    }
                });

                if (goAroundAgain)
                {
                    return await stepContext.ReplaceDialogAsync(nameof(WaterfallDialog));
                }
            }
            else
            {
                await SendMsg(stepContext.Context, "Oops, didn't get that for some reason. Oh well; everything else seemed to work. Thanks & byee!");
            }

        }
        else
        {
            await SendMsg(stepContext.Context, "Oops, can't report feedback for anonymous users. Your login is needed even if we don't report on it. Thanks for letting me know anyway.");
        }

        return await stepContext.EndDialogAsync();
    }
}
