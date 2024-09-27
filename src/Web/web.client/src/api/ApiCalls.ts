import { loadFromApi } from "./ApiLoader";

/*
export const startProcessingJobTitlesReport = async (token: string, skillIds: string[]): Promise<JobTitleReportResponse> => {
  return loadFromApi('Reports/StartProcessingJobTitlesReport', 'POST', token, skillIds)
    .then(async response => {
      const d: JobTitleReportResponse = JSON.parse(response);
      return d;
    })
}
*/