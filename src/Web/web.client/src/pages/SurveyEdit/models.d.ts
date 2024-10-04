interface SurveyPageEditProps {
    page: SurveyPageEditViewModel,
    onPageEdited: Function,
    onPageSave: Function,
    onQuestionEdited: Function,
    onPageDeleted: Function,
    onQuestionDeleted: Function,
    onEditCancel: Function,
}


interface SurveyQuestionProps {
    q: SurveyQuestionDB;
    onQuestionEdited: Function;
    onQuestionDeleted: Function;
}
