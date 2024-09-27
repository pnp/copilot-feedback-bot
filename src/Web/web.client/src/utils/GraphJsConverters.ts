import { ChartData } from "chart.js";
import moment from "moment";
import { ColourPicker } from "./ColourPicker";


// Skills %
export const getGraphDataFromSkillsNameStats = (model: SkillsNameStats): ChartData<"doughnut", number[], string> => {

  if (model.stats.length === 0) {
    return {
      datasets: [],
      labels: []
    };
  }

  const colours = new ColourPicker(0);
  const d: ChartData<"doughnut", number[], string> =
  {
    datasets: [{
      data: model.stats.map((s) => s.totalValue),
      borderColor: "#22233A",
      backgroundColor: model.stats.map(() => colours.charColour())
    }],
    labels: model.stats.map(d => d.name)
  };

  return d;
}

// Company comparison
export const getGraphDataFromCognitiveStatsCompanyComparisonModel = (model: SkillsCompanyComparisonModel): ChartData<"bar", number[], string> => {

  if (model.companyStats.length === 0) {
    return {
      datasets: [],
      labels: []
    };
  }

  const d: ChartData<"bar", number[], string> =
  {
    datasets: [],
    labels: model.allDates.map(d => moment(d).format('yyyy-MM-DD'))
  };


  const colours = new ColourPicker(2);

  model.companyStats.forEach((clientStat: CompanyComparisonScore) => {

    d.datasets.push({
      label: clientStat.name,
      type: 'bar' as const,
      data: clientStat.skillValuesAllDates.map(s => s),
      borderColor: colours.charColour(),
      backgroundColor: colours.charColour(),
    });

  });

  return d;
}
