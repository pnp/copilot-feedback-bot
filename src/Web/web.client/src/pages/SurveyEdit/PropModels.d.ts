import { SurveyPageDTO } from "../../apimodels/Models";

abstract interface PageProps {
    page: SurveyPageDTO,
}

interface EditSurveyQuestionsProps extends PageProps {
    onQuestionEdited: Function,
    onQuestionDeleted: Function
}

interface SurveyPageViewProps extends PageProps {
    onStartEdit: Function,
    onDelete: Function
}

interface SurveyPageAndQuestionsEditProps extends PageProps {
    onPageFieldUpdated: Function,
    onPageSave: Function,
    onQuestionEdited: Function,
    onQuestionDeleted: Function,
    onEditCancel: Function,
}

interface SurveyQuestionProps {
    q: SurveyQuestionDTO;
    onQuestionEdited: Function;
    onQuestionDeleted: Function;
}

interface EditSurveyPageProps extends PageProps { onPageFieldUpdated: Function }
