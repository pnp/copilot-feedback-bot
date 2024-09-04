# DialogFlow Logic Readme

The bots show cards in this order:

New user: 
BotFirstIntro. Contains "sounds good" button (i.e. "start survey"). If they want to start the survey, they are taken to 1st step of existing user.

Existing user:
Straight into survey request (either Teams, Doc, or general survey) - give copilot a rating. 

When they respond, the bot reaction card is picked depending on how they rated copilot. They're asked to provide feedback which they can do in the follow-up questions.
