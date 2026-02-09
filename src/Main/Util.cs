using System;

namespace Main {
public static class Util {
  private static readonly System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo(
    "en-US", false).TextInfo;
  public static string ToTitleCase(string s) {
    return textInfo.ToTitleCase(s.ToLower());
  }

  public static DirectionEnum Combine(DirectionEnum horiz, DirectionEnum vert) {
    if (horiz != DirectionEnum.EAST && horiz != DirectionEnum.WEST && horiz != DirectionEnum.NONE) {
      throw new ArgumentException("Horizontal direction must be EAST or WEST");
    }
    if (vert != DirectionEnum.NORTH && vert != DirectionEnum.SOUTH && vert != DirectionEnum.NONE) {
      throw new ArgumentException("Vertical direction must be NORTH or SOUTH");
    }

    if (horiz == DirectionEnum.NONE) { return vert; }
    if (vert == DirectionEnum.NONE) { return horiz; }

    if (horiz == DirectionEnum.EAST && vert == DirectionEnum.NORTH) {
      return DirectionEnum.NORTHEAST;
    } else if (horiz == DirectionEnum.EAST && vert == DirectionEnum.SOUTH) {
      return DirectionEnum.SOUTHEAST;
    } else if (horiz == DirectionEnum.WEST && vert == DirectionEnum.NORTH) {
      return DirectionEnum.NORTHWEST;
    } else if (horiz == DirectionEnum.WEST && vert == DirectionEnum.SOUTH) {
      return DirectionEnum.SOUTHWEST;
    } else {
      throw new ArgumentException("Invalid combination of directions");
    }
  }
}
}