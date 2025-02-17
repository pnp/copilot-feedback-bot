import moment from "moment";
import { ChartView } from "../apimodels/Enums";

export function avg(array: number[]): number {
  let sum = 0;
  for (let i = 0; i < array.length; i++) {
    sum += array[i];
  }
  return sum / array.length
}

export function getDateFromNow(length: ChartView): Date {
  let dateFrom = moment(new Date()).add(-7, 'days').toDate();
  if (length === ChartView.OneWeek) {
    // Already defaults to this
  }
  else if (length === ChartView.OneMonth) {
    dateFrom = moment(new Date()).add(-1, 'months').toDate();
  }
  else if (length === ChartView.ThreeMonths) {
    dateFrom = moment(new Date()).add(-3, 'months').toDate();
  }
  else if (length === ChartView.SixMonths) {
    dateFrom = moment(new Date()).add(-6, 'months').toDate();
  }
  else if (length === ChartView.OneYear) {
    dateFrom = moment(new Date()).add(-1, 'years').toDate();
  }
  else {
    throw new Error("Invalid date length");
  }

  return dateFrom;
}

export function getRootUrl(url: string): string {
  let parser = new URL(url);
  let protocol = parser.protocol; // e.g. "https:"
  let domainName = parser.hostname; // e.g. "example.com"
  let port = parser.port; // e.g. "8080"
  if (port && port !== "80" && port !== "443") {
    domainName = `${domainName}:${port}`;
  }
  let rootUrl = `${protocol}//${domainName}`;
  return rootUrl;
}
