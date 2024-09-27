import tinycolor from "tinycolor2";

export class ColourPicker {
  lastColourIndex = 0;
  allColours : string[] = [];

  constructor(startIdx: number | undefined) {
    if (startIdx) {
      this.lastColourIndex = startIdx;
    }
    this.allColours = this.generateColors(ColourPicker.chartColours, 50);
  }
  static chartColours: string[] =
    [
      "#C8AC72",   // gold
      "#C88172",  // red
      "#B9C872", // Green
          "#728EC8", // Blue,
          "#777684" // Dark grey
          ,"#323232" // Darker grey
    ];

  charColour() {

    const nextColour = ColourPicker.hexToRGB(this.allColours[this.lastColourIndex], 0.8);
    this.lastColourIndex++;
    if (this.lastColourIndex === this.allColours.length) {
      this.lastColourIndex = 0;
    }
    return nextColour;
  };

  generateColors(baseColors: string[], numColors: number): string[] {
    let colors: string[] = [];
    for (let i = 0; i < numColors; i++) {
        // Get base color
        let baseColor = tinycolor(baseColors[i % baseColors.length]);

        // Generate new color by changing the hue
        let newColor = baseColor.spin(i * 360 / numColors);

        // Add new color to list
        colors.push(newColor.toHexString());
    }
    return colors;
}

  static hexToRGB(hex: string, alpha?: number) {
    var r = parseInt(hex.slice(1, 3), 16),
      g = parseInt(hex.slice(3, 5), 16),
      b = parseInt(hex.slice(5, 7), 16);

    if (alpha) {
      return "rgba(" + r + ", " + g + ", " + b + ", " + alpha + ")";
    } else {
      return "rgb(" + r + ", " + g + ", " + b + ")";
    }
  };

}
