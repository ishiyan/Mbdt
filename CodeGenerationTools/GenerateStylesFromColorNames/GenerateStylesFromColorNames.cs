using System;
using System.IO;
using System.Collections.Generic;

public static class GenerateStylesFromColorNames
{
    public static void Main()
    {
        var listEnumeration = new List<string>(8192);
        var listDescription = new List<string>(8192);
        var list = new List<string>(8192);

        File.WriteAllLines("PredefinedLineStyle.cs", GenerateLineStylesEnum(styleColors, listEnumeration, listDescription));
        list.AddRange(GenerateLineStylesXaml(styleColors, listEnumeration, listDescription));
        File.WriteAllLines("CandlestickChartLineStyles.xaml", list);
        list.Clear();

        File.WriteAllLines("PredefinedBandStyle.cs", GenerateBandStylesEnum(styleColors, listEnumeration, listDescription));
        list.AddRange(GenerateBandStylesXaml(styleColors, listEnumeration, listDescription));
        File.WriteAllLines("CandlestickChartBandStyles.xaml", list);
        list.Clear();

        File.WriteAllLines("PredefinedMarkerStyle.cs", GenerateMarkerStylesEnum(styleColors, listEnumeration, listDescription));
        list.AddRange(GenerateMarkerStylesXaml(styleColors, listEnumeration, listDescription));
        File.WriteAllLines("CandlestickChartMarkerStyles.xaml", list);
        list.Clear();

        File.WriteAllLines("PredefinedFunctionStyle.cs", GenerateFunctionStylesEnum(styleColors, listEnumeration, listDescription));
        list.AddRange(GenerateFunctionStylesXaml(styleColors, listEnumeration, listDescription));
        File.WriteAllLines("FunctionChartFunctionStyles.xaml", list);
        list.Clear();

        File.WriteAllLines("PredefinedGuideLineStyle.cs", GenerateGuideLineStylesEnum(styleColors, listEnumeration, listDescription));
        list.AddRange(GenerateGuideLineStylesXaml(styleColors, listEnumeration, listDescription));
        File.WriteAllLines("FunctionChartGuideLineStyles.xaml", list);
        list.Clear();
    }

    private struct StyleColor
    {
        public string Name, Color;
        public bool  ExcludeFromBandStyle;
    }

    private static readonly StyleColor[] styleColors =
    {
        #region Green
        new StyleColor{Name="GreenYellow", Color="#ADFF2F"},
        //new StyleColor{Name="Chartreuse", Color="#7FFF00"},
        new StyleColor{Name="LawnGreen", Color="#7CFC00"},
        new StyleColor{Name="Lime", Color="#00FF00"},
        new StyleColor{Name="LimeGreen", Color="#32CD32"},
        new StyleColor{Name="PaleGreen", Color="#98FB98"},
        //new StyleColor{Name="LightGreen", Color="#90EE90"},
        new StyleColor{Name="MediumSpringGreen", Color="#00FA9A"},
        new StyleColor{Name="SpringGreen", Color="#00FF7F"},
        new StyleColor{Name="MediumSeaGreen", Color="#3CB371"},
        new StyleColor{Name="SeaGreen", Color="#2E8B57"},
        new StyleColor{Name="ForestGreen", Color="#228B22"},
        //new StyleColor{Name="Green", Color="#008000"},
        new StyleColor{Name="DarkGreen", Color="#006400"},
        new StyleColor{Name="YellowGreen", Color="#9ACD32"},
        new StyleColor{Name="OliveDrab", Color="#6B8E23"},
        new StyleColor{Name="Olive", Color="#808000"},
        new StyleColor{Name="DarkOliveGreen", Color="#556B2F"},
        new StyleColor{Name="MediumAquamarine", Color="#66CDAA"},
        new StyleColor{Name="DarkSeaGreen", Color="#8FBC8F"},
        new StyleColor{Name="LightSeaGreen", Color="#20B2AA"},
        //new StyleColor{Name="DarkCyan", Color="#008B8B"},
        new StyleColor{Name="Teal", Color="#008080"},
        #endregion
        #region Blue/cyan
        new StyleColor{Name="Cyan", Color="#00FFFF"},
        //new StyleColor{Name="LightCyan", Color="#E0FFFF"},
        new StyleColor{Name="PaleTurquoise", Color="#AFEEEE"},
        new StyleColor{Name="Aquamarine", Color="#7FFFD4"},
        new StyleColor{Name="Turquoise", Color="#40E0D0"},
        //new StyleColor{Name="MediumTurquoise", Color="#48D1CC"},
        //new StyleColor{Name="DarkTurquoise", Color="#00CED1"},
        new StyleColor{Name="CadetBlue", Color="#5F9EA0"},
        new StyleColor{Name="SteelBlue", Color="#4682B4"},
        new StyleColor{Name="LightSteelBlue", Color="#B0C4DE"},
        //new StyleColor{Name="PowderBlue", Color="#B0E0E6"},
        new StyleColor{Name="LightBlue", Color="#ADD8E6"},
        new StyleColor{Name="SkyBlue", Color="#87CEEB"},
        //new StyleColor{Name="LightSkyBlue", Color="#87CEFA"},
        new StyleColor{Name="DeepSkyBlue", Color="#00BFFF"},
        new StyleColor{Name="DodgerBlue", Color="#1E90FF"},
        new StyleColor{Name="CornflowerBlue", Color="#6495ED"},
        new StyleColor{Name="RoyalBlue", Color="#4169E1"},
        new StyleColor{Name="Blue", Color="#0000FF"},
        //new StyleColor{Name="MediumBlue", Color="#0000CD"},
        new StyleColor{Name="DarkBlue", Color="#00008B"},
        //new StyleColor{Name="Navy", Color="#000080"},
        new StyleColor{Name="MidnightBlue", Color="#191970"},
        #endregion
        #region Purple
        //new StyleColor{Name="MediumSlateBlue", Color="#7B68EE"},
        new StyleColor{Name="SlateBlue", Color="#6A5ACD"},
        new StyleColor{Name="DarkSlateBlue", Color="#483D8B"},
        new StyleColor{Name="Indigo", Color="#4B0082"},
        new StyleColor{Name="Purple", Color="#800080"},
        //new StyleColor{Name="DarkMagenta", Color="#8B008B"},
        new StyleColor{Name="DarkOrchid", Color="#9932CC"},
        new StyleColor{Name="DarkViolet", Color="#9400D3"},
        //new StyleColor{Name="BlueViolet", Color="#8A2BE2"},
        new StyleColor{Name="MediumPurple", Color="#9370DB"},
        new StyleColor{Name="MediumOrchid", Color="#BA55D3"},
        new StyleColor{Name="Magenta", Color="#FF00FF"},
        new StyleColor{Name="Orchid", Color="#DA70D6"},
        new StyleColor{Name="Violet", Color="#EE82EE"},
        new StyleColor{Name="Plum", Color="#DDA0DD"},
        //new StyleColor{Name="Thistle", Color="#D8BFD8"},
        //new StyleColor{Name="Lavender", Color="#E6E6FA"},
        #endregion
        #region Pink
        new StyleColor{Name="Pink", Color="#FFC0CB"},
        //new StyleColor{Name="LightPink", Color="#FFB6C1"},
        new StyleColor{Name="HotPink", Color="#FF69B4"},
        new StyleColor{Name="DeepPink", Color="#FF1493"},
        new StyleColor{Name="MediumVioletRed", Color="#C71585"},
        new StyleColor{Name="PaleVioletRed", Color="#DB7093"},
        #endregion
        #region Red
        new StyleColor{Name="IndianRed", Color="#CD5C5C"},
        new StyleColor{Name="LightCoral", Color="#F08080"},
        new StyleColor{Name="Salmon", Color="#FA8072"},
        new StyleColor{Name="DarkSalmon", Color="#E9967A"},
        new StyleColor{Name="LightSalmon", Color="#FFA07A"},
        new StyleColor{Name="Red", Color="#FF0000"},
        new StyleColor{Name="Crimson", Color="#DC143C"},
        new StyleColor{Name="FireBrick", Color="#B22222"},
        new StyleColor{Name="DarkRed", Color="#8B0000"},
        #endregion
        #region Brown
        new StyleColor{Name="Maroon", Color="#800000"},
        new StyleColor{Name="Brown", Color="#A52A2A"},
        new StyleColor{Name="Sienna", Color="#A0522D"},
        new StyleColor{Name="SaddleBrown", Color="#8B4513"},
        new StyleColor{Name="Chocolate", Color="#D2691E"},
        new StyleColor{Name="Peru", Color="#CD853F"},
        new StyleColor{Name="DarkGoldenrod", Color="#B8860B"},
        new StyleColor{Name="Goldenrod", Color="#DAA520"},
        new StyleColor{Name="SandyBrown", Color="#F4A460"},
        new StyleColor{Name="RosyBrown", Color="#BC8F8F"},
        new StyleColor{Name="Tan", Color="#D2B48C"},
        new StyleColor{Name="BurlyWood", Color="#DEB887"},
        //new StyleColor{Name="Wheat", Color="#F5DEB3"},
        //new StyleColor{Name="NavajoWhite", Color="#FFDEAD"},
        //new StyleColor{Name="Bisque", Color="#FFE4C4"},
        //new StyleColor{Name="BlanchedAlmond", Color="#FFEBCD"},
        //new StyleColor{Name="Cornsilk", Color="#FFF8DC"},
        #endregion
        #region Orange
        new StyleColor{Name="Coral", Color="#FF7F50"},
        new StyleColor{Name="Tomato", Color="#FF6347"},
        new StyleColor{Name="OrangeRed", Color="#FF4500"},
        new StyleColor{Name="DarkOrange", Color="#FF8C00"},
        new StyleColor{Name="Orange", Color="#FFA500"},
        #endregion
        #region Yellow
        new StyleColor{Name="Gold", Color="#FFD700"},
        new StyleColor{Name="Yellow", Color="#FFFF00"},
        //new StyleColor{Name="LightYellow", Color="#FFFFE0"},
        //new StyleColor{Name="LemonChiffon", Color="#FFFACD"},
        //new StyleColor{Name="LightGoldenrodYellow", Color="#FAFAD2"},
        //new StyleColor{Name="PapayaWhip", Color="#FFEFD5"},
        //new StyleColor{Name="Moccasin", Color="#FFE4B5"},
        //new StyleColor{Name="PeachPuff", Color="#FFDAB9"},
        //new StyleColor{Name="PaleGoldenrod", Color="#EEE8AA"},
        new StyleColor{Name="Khaki", Color="#F0E68C"},
        new StyleColor{Name="DarkKhaki", Color="#BDB76B"},
        #endregion
        #region White
        new StyleColor{Name="White", Color="#FFFFFF", ExcludeFromBandStyle=true},
        //new StyleColor{Name="Snow", Color="#FFFAFA", ExcludeFromBandStyle=true},
        //new StyleColor{Name="Honeydew", Color="#F0FFF0", ExcludeFromBandStyle=true},
        //new StyleColor{Name="MintCream", Color="#F5FFFA", ExcludeFromBandStyle=true},
        //new StyleColor{Name="Azure", Color="#F0FFFF", ExcludeFromBandStyle=true},
        //new StyleColor{Name="AliceBlue", Color="#F0F8FF", ExcludeFromBandStyle=true},
        //new StyleColor{Name="GhostWhite", Color="#F8F8FF", ExcludeFromBandStyle=true},
        //new StyleColor{Name="WhiteSmoke", Color="#F5F5F5", ExcludeFromBandStyle=true},
        //new StyleColor{Name="Seashell", Color="#FFF5EE", ExcludeFromBandStyle=true},
        //new StyleColor{Name="Beige", Color="#F5F5DC", ExcludeFromBandStyle=true},
        //new StyleColor{Name="OldLace", Color="#FDF5E6", ExcludeFromBandStyle=true},
        //new StyleColor{Name="FloralWhite", Color="#FFFAF0", ExcludeFromBandStyle=true},
        //new StyleColor{Name="Ivory", Color="#FFFFF0", ExcludeFromBandStyle=true},
        //new StyleColor{Name="AntiqueWhite", Color="#FAEBD7", ExcludeFromBandStyle=true},
        //new StyleColor{Name="Linen", Color="#FAF0E6", ExcludeFromBandStyle=true},
        //new StyleColor{Name="LavenderBlush", Color="#FFF0F5", ExcludeFromBandStyle=true},
        //new StyleColor{Name="MistyRose", Color="#FFE4E1", ExcludeFromBandStyle=true},
        #endregion
        #region Gray
        //new StyleColor{Name="Gainsboro", Color="#DCDCDC", ExcludeFromBandStyle=true},
        //new StyleColor{Name="LightGrey", Color="#D3D3D3", ExcludeFromBandStyle=true},
        new StyleColor{Name="Silver", Color="#C0C0C0", ExcludeFromBandStyle=true},
        //new StyleColor{Name="DarkGray", Color="#A9A9A9", ExcludeFromBandStyle=true},
        new StyleColor{Name="Gray", Color="#808080", ExcludeFromBandStyle=true},
        //new StyleColor{Name="DimGray", Color="#696969", ExcludeFromBandStyle=true},
        //new StyleColor{Name="LightSlateGray", Color="#778899", ExcludeFromBandStyle=true},
        //new StyleColor{Name="SlateGray", Color="#708090", ExcludeFromBandStyle=true},
        //new StyleColor{Name="DarkSlateGray", Color="#2F4F4F", ExcludeFromBandStyle=true},
        new StyleColor{Name="Black", Color="#000000", ExcludeFromBandStyle=true}
        #endregion
    };

    #region Line styles
    private static IEnumerable<string> GenerateLineStylesXaml(StyleColor[] enumerable, List<string> listEnumeration, List<string> listDescription,
        string lineOpacity="0.9", string fillOpacity="0.4", string markerOpacity="1.0")
    {
        var list = new List<string>(4096)
        {
            "<ResourceDictionary",
            "    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
            "    xmlns:PresentationOptions=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation/options\"",
            "    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
            "    xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
            "    xmlns:local=\"clr-namespace:Mbst.Charts\"",
            "    mc:Ignorable=\"PresentationOptions\">",
            "    <!-- ======================== Line Styles. Automatically generated together with an enumeration for the line styles. ======================== -->",
            "    <local:LineStyleList x:Key=\"LineStyles\">"
        };
        // ReSharper disable ImplicitlyCapturedClosure
        Action<string, string> beginStyle = delegate(string e, string d)
        {
            list.Add($"        <local:LineStyle Enumeration=\"{e}\">");
        };
        Action<string, string> beginStyleMax6 = delegate(string e, string d)
        {
            list.Add($"        <local:LineStyle Enumeration=\"{e}\" MarkerMinimalWidth=\"1\" MarkerMaximalWidth=\"6\">");
        };
        Action<string, string> beginStyleMax7 = delegate(string e, string d)
        {
            list.Add($"        <local:LineStyle Enumeration=\"{e}\" MarkerMinimalWidth=\"1\" MarkerMaximalWidth=\"7\">");
        };
        Action<string, string> beginStyleMax10 = delegate(string e, string d)
        {
            list.Add($"        <local:LineStyle Enumeration=\"{e}\" MarkerMinimalWidth=\"1\" MarkerMaximalWidth=\"10\">");
        };
        Action endStyle = delegate
        {
            list.Add("        </local:LineStyle>");
        };
        Action<StyleColor> line = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Pen>");
            list.Add("                <Pen Thickness=\"0.9\" DashStyle=\"{x:Static DashStyles.Solid}\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <Pen.Brush>");
            list.Add($"                        <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{lineOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                    </Pen.Brush>");
            list.Add("                </Pen>");
            list.Add("            </local:LineStyle.Pen>");
        };
        Action<StyleColor> fill = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Fill>");
            list.Add($"                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{fillOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("            </local:LineStyle.Fill>");
        };
        Action<StyleColor> circle = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Marker>");
            list.Add("                <DrawingBrush Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add($"                                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{markerOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <EllipseGeometry RadiusX=\"50\" RadiusY=\"50\" Center=\"50,50\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                        </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                </DrawingBrush>");
            list.Add("            </local:LineStyle.Marker>");
        };
        Action<StyleColor> asterisk = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Marker>");
            list.Add("                <DrawingBrush Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add($"                                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{markerOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!-- asterisk -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M32.259997,13.009977L23.652998,13.009977 29.737997,6.9299784 25.331998,2.5199797 19.245998,8.6099779 19.245998,-1.9531244E-05 13.013998,-1.9531244E-05 13.013998,8.6099779 6.9279987,2.5199797 2.520999,6.9299784 8.6079986,13.009977 -7.9345698E-07,13.009977 -7.9345698E-07,19.249975 8.6079986,19.249975 2.520999,25.329973 6.9279987,29.739971 13.013998,23.649973 13.013998,32.259971 19.245998,32.259971 19.245998,23.649973 25.331998,29.739971 29.737997,25.329973 23.652998,19.249975 32.259997,19.249975 32.259997,13.009977z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                        </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                </DrawingBrush>");
            list.Add("            </local:LineStyle.Marker>");
        };
        Action<StyleColor> star = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Marker>");
            list.Add("                <DrawingBrush Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add($"                                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{markerOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!-- star, a = width (100) M0,0 La,0 La/2,a -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M12.763,0L15.824005,9.2739811L25.527,9.2739811L17.638763,15.004915L20.652,24.278L12.763499,18.546873L4.8750005,24.277998L7.887928,15.004692L0,9.273982L9.7000046,9.273982z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                        </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                </DrawingBrush>");
            list.Add("            </local:LineStyle.Marker>");
        };
        Action<StyleColor> triangleUp = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Marker>");
            list.Add("                <DrawingBrush Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add($"                                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{markerOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!-- trinagle up, a = width (100) Ma/2,0 La,a L0,a -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M50,0L100,100L0,100z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                        </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                </DrawingBrush>");
            list.Add("            </local:LineStyle.Marker>");
        };
        Action<StyleColor> triangleDown = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Marker>");
            list.Add("                <DrawingBrush Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add($"                                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{markerOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!-- trinagle down, a = width (100) M0,0 La,0 La/2,a -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M0,0L100,0L50,100z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                        </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                </DrawingBrush>");
            list.Add("            </local:LineStyle.Marker>");
        };
        Action<StyleColor> rhomb = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Marker>");
            list.Add("                <DrawingBrush Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add($"                                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{markerOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!-- rhomb, a = width (100) Ma/2,0 La,a/2 La/2,a L0,a/2 -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M50,0L100,50L50,100L0,50z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                        </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                </DrawingBrush>");
            list.Add("            </local:LineStyle.Marker>");
        };
        Action<StyleColor> square = delegate(StyleColor v)
        {
            list.Add("            <local:LineStyle.Marker>");
            list.Add("                <DrawingBrush Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add($"                                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{markerOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!-- rhomb, a = width (100) Ma/2,0 La,a/2 La/2,a L0,a/2 -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M50,0L100,50L50,100L0,50z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                        </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                </DrawingBrush>");
            list.Add("            </local:LineStyle.Marker>");
        };
        // ReSharper restore ImplicitlyCapturedClosure

        int i = 0, j = 0;
        foreach (var v in enumerable)
        {
            list.Add($"        <!-- {v.Name} {v.Color} -->");
            beginStyle(listEnumeration[i++], listDescription[j++]);
            line(v);
            endStyle();
            beginStyle(listEnumeration[i++], listDescription[j++]);
            line(v);
            fill(v);
            endStyle();
            beginStyle(listEnumeration[i++], listDescription[j++]);
            fill(v);
            endStyle();

            #region xxx{Line}{Fill}Circle
            beginStyleMax6(listEnumeration[i++], listDescription[j++]);
            circle(v);
            endStyle();
            beginStyleMax6(listEnumeration[i++], listDescription[j++]);
            line(v);
            circle(v);
            endStyle();
            //beginStyleMax6(listEnumeration[i++], listDescription[j++]);
            //fill(v);
            //circle(v);
            //endStyle();
            //beginStyleMax6(listEnumeration[i++], listDescription[j++]);
            //line(v);
            //fill(v);
            //circle(v);
            //endStyle();
            #endregion

            #region xxx{Line}{Fill}Asterisk
            beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            asterisk(v);
            endStyle();
            beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            line(v);
            asterisk(v);
            endStyle();
            //beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            //fill(v);
            //asterisk(v);
            //endStyle();
            //beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            //line(v);
            //fill(v);
            //asterisk(v);
            //endStyle();
            #endregion

            #region xxx{Line}{Fill}Star
            beginStyleMax10(listEnumeration[i++], listDescription[j++]);
            star(v);
            endStyle();
            beginStyleMax10(listEnumeration[i++], listDescription[j++]);
            line(v);
            star(v);
            endStyle();
            //beginStyleMax10(listEnumeration[i++], listDescription[j++]);
            //fill(v);
            //star(v);
            //endStyle();
            //beginStyleMax10(listEnumeration[i++], listDescription[j++]);
            //line(v);
            //fill(v);
            //star(v);
            //endStyle();
            #endregion

            #region xxx{Line}{Fill}TriangleUp
            beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            triangleUp(v);
            endStyle();
            beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            line(v);
            triangleUp(v);
            endStyle();
            //beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            //fill(v);
            //triangleUp(v);
            //endStyle();
            //beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            //line(v);
            //fill(v);
            //triangleUp(v);
            //endStyle();
            #endregion

            #region xxx{Line}{Fill}TriangleDown
            beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            triangleDown(v);
            endStyle();
            beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            line(v);
            triangleDown(v);
            endStyle();
            //beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            //fill(v);
            //triangleDown(v);
            //endStyle();
            //beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            //line(v);
            //fill(v);
            //triangleDown(v);
            //endStyle();
            #endregion

            #region xxx{Line}{Fill}Rhomb
            beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            rhomb(v);
            endStyle();
            beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            line(v);
            rhomb(v);
            endStyle();
            //beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            //fill(v);
            //rhomb(v);
            //endStyle();
            //beginStyleMax7(listEnumeration[i++], listDescription[j++]);
            //line(v);
            //fill(v);
            //rhomb(v);
            //endStyle();
            #endregion

            #region xxx{Line}{Fill}Square
            beginStyleMax6(listEnumeration[i++], listDescription[j++]);
            square(v);
            endStyle();
            beginStyleMax6(listEnumeration[i++], listDescription[j++]);
            line(v);
            square(v);
            endStyle();
            //beginStyleMax6(listEnumeration[i++], listDescription[j++]);
            //fill(v);
            //square(v);
            //endStyle();
            //beginStyleMax6(listEnumeration[i++], listDescription[j++]);
            //line(v);
            //fill(v);
            //square(v);
            //endStyle();
            #endregion
        }
        list.Add("    </local:LineStyleList>");
        list.Add("</ResourceDictionary>");
        return list;
    }

    private static IEnumerable<string> GenerateLineStylesEnum(StyleColor[] enumerable, List<string> listEnumeration, List<string> listDescription)
    {
        listEnumeration.Clear();
        listDescription.Clear();
        bool first = true;
        var list = new List<string>(4096)
        {
            "namespace Mbst.Charts",
            "{",
            "    /// <summary>",
            "    /// Enumerates predefined line styles.",
            "    /// </summary>",
            "    public enum PredefinedLineStyle",
            "    {",
            "        // This code is automatically generated together with a XAML code for the line styles.",
            ""
        };
        foreach (var v in enumerable)
        {
            string enumeration = string.Concat(v.Name, "Line");
            string description = string.Concat("A ", v.Name, " line without an interior fill and a marker.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            if (first)
            {
                first = false;
                list.Add($"        {enumeration} = 0,");
            }
            else
                list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineFill");
            description = string.Concat("A ", v.Name, " line and an interior fill without a marker.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "Fill");
            description = string.Concat("A ", v.Name, " interior fill without a bounding line and a marker.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            #region Circle
            list.Add("");
            enumeration = string.Concat(v.Name, "Circle");
            description = string.Concat("A ", v.Name, " circle (●) marker without a line and an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineCircle");
            description = string.Concat("A ", v.Name, " circle (●) marker and a line without an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            //list.Add("");
            //enumeration = string.Concat(v.Name, "FillCircle");
            //description = string.Concat("A ", v.Name, " circle (●) marker and an interior fill without a bounding line.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            //list.Add("");
            //enumeration = string.Concat(v.Name, "LineFillCircle");
            //description = string.Concat("A ", v.Name, " circle (●) marker, a line, and an interior fill.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            #endregion
            #region Asterisk
            list.Add("");
            enumeration = string.Concat(v.Name, "Asterisk");
            description = string.Concat("An ", v.Name, " asterisk (⁕) marker without a line and an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineAsterisk");
            description = string.Concat("An ", v.Name, " asterisk (⁕) marker and a line without an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            //list.Add("");
            //enumeration = string.Concat(v.Name, "FillAsterisk");
            //description = string.Concat("An ", v.Name, " asterisk (⁕) marker and an interior fill without a bounding line.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            //list.Add("");
            //enumeration = string.Concat(v.Name, "LineFillAsterisk");
            //description = string.Concat("An ", v.Name, " asterisk (⁕) marker, a line, and an interior fill.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            #endregion
            #region Star
            list.Add("");
            enumeration = string.Concat(v.Name, "Star");
            description = string.Concat("A ", v.Name, " star (★) marker without a line and an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineStar");
            description = string.Concat("A ", v.Name, " star (★) marker and a line without an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            //list.Add("");
            //enumeration = string.Concat(v.Name, "FillStar");
            //description = string.Concat("A ", v.Name, " star (★) marker and an interior fill without a bounding line.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            //list.Add("");
            //enumeration = string.Concat(v.Name, "LineFillStar");
            //description = string.Concat("A ", v.Name, " star (★) marker, a line, and an interior fill.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            #endregion
            #region TriangleUp
            list.Add("");
            enumeration = string.Concat(v.Name, "TriangleUp");
            description = string.Concat("A ", v.Name, " triangle up (▲) marker without a line and an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineTriangleUp");
            description = string.Concat("A ", v.Name, " triangle up (▲) marker and a line without an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            //list.Add("");
            //enumeration = string.Concat(v.Name, "FillTriangleUp");
            //description = string.Concat("A ", v.Name, " triangle up (▲) marker and an interior fill without a bounding line.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            //list.Add("");
            //enumeration = string.Concat(v.Name, "LineFillTriangleUp");
            //description = string.Concat("A ", v.Name, " triangle up (▲) marker, a line, and an interior fill.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            #endregion
            #region TriangleDown
            list.Add("");
            enumeration = string.Concat(v.Name, "TriangleDown");
            description = string.Concat("A ", v.Name, " triangle down (▼) marker without a line and an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineTriangleDown");
            description = string.Concat("A ", v.Name, " triangle down (▼) marker and a line without an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            //list.Add("");
            //enumeration = string.Concat(v.Name, "FillTriangleDown");
            //description = string.Concat("A ", v.Name, " triangle down (▼) marker and an interior fill without a bounding line.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            //list.Add("");
            //enumeration = string.Concat(v.Name, "LineFillTriangleDown");
            //description = string.Concat("A ", v.Name, " triangle down (▼) marker, a line, and an interior fill.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            #endregion
            #region Rhomb
            list.Add("");
            enumeration = string.Concat(v.Name, "Rhomb");
            description = string.Concat("A ", v.Name, " rhomb (⧫) marker without a line and an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineRhomb");
            description = string.Concat("A ", v.Name, " rhomb (⧫) marker and a line without an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            //list.Add("");
            //enumeration = string.Concat(v.Name, "FillRhomb");
            //description = string.Concat("A ", v.Name, " rhomb (⧫) marker and an interior fill without a bounding line.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            //list.Add("");
            //enumeration = string.Concat(v.Name, "LineFillRhomb");
            //description = string.Concat("A ", v.Name, " rhomb (⧫) marker, a line, and an interior fill.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            #endregion
            #region Square
            list.Add("");
            enumeration = string.Concat(v.Name, "Square");
            description = string.Concat("A ", v.Name, " square (■) marker without a line and an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineSquare");
            description = string.Concat("A ", v.Name, " square (■) marker and a line without an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            //list.Add("");
            //enumeration = string.Concat(v.Name, "FillSquare");
            //description = string.Concat("A ", v.Name, " square (■) marker and an interior fill without a bounding line.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            //list.Add("");
            //enumeration = string.Concat(v.Name, "LineFillSquare");
            //description = string.Concat("A ", v.Name, " square (■) marker, a line, and an interior fill.");
            //listEnumeration.Add(enumeration);
            //listDescription.Add(description);
            //list.Add("        /// <summary>");
            //list.Add(string.Format("        /// {0}", description));
            //list.Add("        /// </summary>");
            //list.Add(string.Format("        {0},", enumeration));
            #endregion
            list.Add("");
        }
        list.Add("    }");
        list.Add("}");
        return list;
    }
    #endregion

    #region Band styles
    private static IEnumerable<string> GenerateBandStylesXaml(StyleColor[] enumerable, List<string> listEnumeration, List<string> listDescription,
        string lineOpacity = "0.9", string fillOpacity = "0.4"/*, string markerOpacity = "1.0"*/)
    {
        var list = new List<string>(4096)
        {
            "<ResourceDictionary",
            "    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
            "    xmlns:PresentationOptions=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation/options\"",
            "    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
            "    xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
            "    xmlns:local=\"clr-namespace:Mbst.Charts\"",
            "    mc:Ignorable=\"PresentationOptions\">",
            "    <!-- ======================== Band Styles. Automatically generated together with an enumeration for the band styles. ======================== -->",
            "    <local:BandStyleList x:Key=\"BandStyles\">"
        };
        // ReSharper disable InconsistentNaming
        // ReSharper disable ConvertToLambdaExpression
        // ReSharper disable ImplicitlyCapturedClosure
        Action<string, string> BeginStyle = (e, d) =>
        {
            list.Add($"        <local:BandStyle Enumeration=\"{e}\">");
        };
        Action EndStyle = () =>
        {
            list.Add("        </local:BandStyle>");
        };
        // ReSharper disable once UnusedVariable
        Action<StyleColor> Line = v =>
        {
            list.Add("            <local:BandStyle.Pen>");
            list.Add("                <Pen Thickness=\"0.9\" DashStyle=\"{x:Static DashStyles.Solid}\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <Pen.Brush>");
            list.Add($"                        <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{lineOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                    </Pen.Brush>");
            list.Add("                </Pen>");
            list.Add("            </local:BandStyle.Pen>");
        };
        // ReSharper disable once UnusedVariable
        Action<StyleColor> SecondLine = v =>
        {
            list.Add("            <local:BandStyle.SecondPen>");
            list.Add("                <Pen Thickness=\"0.9\" DashStyle=\"{x:Static DashStyles.Solid}\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <Pen.Brush>");
            list.Add($"                        <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{lineOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                    </Pen.Brush>");
            list.Add("                </Pen>");
            list.Add("            </local:BandStyle.SecondPen>");
        };
        Action<StyleColor> Fill = v =>
        {
            list.Add("            <local:BandStyle.Fill>");
            list.Add($"                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{fillOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("            </local:BandStyle.Fill>");
        };
        Action<StyleColor> InvertedFill = v =>
        {
            list.Add("            <local:BandStyle.InvertedFill>");
            list.Add($"                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"{fillOpacity}\" PresentationOptions:Freeze=\"True\" />");
            list.Add("            </local:BandStyle.InvertedFill>");
        };
        // ReSharper restore ImplicitlyCapturedClosure
        // ReSharper restore ConvertToLambdaExpression
        // ReSharper restore InconsistentNaming

        int i = 0;
        foreach (var v in enumerable)
        {
            if (v.ExcludeFromBandStyle)
                continue;
            string e = listEnumeration[i];
            string d = listDescription[i];
            ++i;
            list.Add($"        <!-- {v.Name} {v.Color} -->");
            BeginStyle(e, d);
            Fill(v);
            EndStyle();
        }
        foreach (var v in enumerable)
        {
            if (v.ExcludeFromBandStyle)
                continue;
            foreach (var s in enumerable)
            {
                if (s.ExcludeFromBandStyle)
                    continue;
                if (!v.Color.Equals(s.Color))
                {
                    string e = listEnumeration[i];
                    string d = listDescription[i];
                    ++i;
                    list.Add($"        <!-- {v.Name} {v.Color} Inverted {s.Name} {s.Color}-->");
                    BeginStyle(e, d);
                    Fill(v);
                    InvertedFill(s);
                    EndStyle();
                }
            }
        }
        list.Add("    </local:BandStyleList>");
        list.Add("</ResourceDictionary>");
        return list;
    }

    private static IEnumerable<string> GenerateBandStylesEnum(StyleColor[] enumerable, List<string> listEnumeration, List<string> listDescription)
    {
        listEnumeration.Clear();
        listDescription.Clear();
        bool first = true;
        var list = new List<string>(4096)
        {
            "namespace Mbst.Charts",
            "{",
            "    /// <summary>",
            "    /// Enumerates predefined band styles.",
            "    /// </summary>", "    public enum PredefinedBandStyle",
            "    {",
            "        // This code is automatically generated together with a XAML code for the band styles."
        };
        foreach (var v in enumerable)
        {
            if (v.ExcludeFromBandStyle)
                continue;
            list.Add("");
            string enumeration = string.Concat(v.Name, "Fill");
            string description = string.Concat("A ", v.Name, " interior fill without an inverted interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            if (first)
            {
                first = false;
                list.Add($"        {enumeration} = 0,");
            }
            else
                list.Add($"        {enumeration},");
        }
        foreach (var v in enumerable)
        {
            if (v.ExcludeFromBandStyle)
                continue;
            foreach (var s in enumerable)
            {
                if (s.ExcludeFromBandStyle)
                    continue;
                if (!v.Color.Equals(s.Color))
                {
                    list.Add("");
                    string enumeration = string.Concat(v.Name, "Fill", s.Name, "Inverted");
                    string description = string.Concat("A ", v.Name, " interior fill with a ", s.Name, " inverted interior fill.");
                    listEnumeration.Add(enumeration);
                    listDescription.Add(description);
                    list.Add("        /// <summary>");
                    list.Add($"        /// {description}");
                    list.Add("        /// </summary>");
                    list.Add($"        {enumeration},");
                }
            }
        }
        list.Add("    }");
        list.Add("}");
        return list;
    }
    #endregion

    #region Marker styles
    private static IEnumerable<string> GenerateMarkerStylesXaml(StyleColor[] enumerable, List<string> listEnumeration, List<string> listDescription)
    {
        var list = new List<string>(4096)
        {
            "<ResourceDictionary",
            "    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
            "    xmlns:PresentationOptions=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation/options\"",
            "    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
            "    xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
            "    xmlns:local=\"clr-namespace:Mbst.Charts\"",
            "    mc:Ignorable=\"PresentationOptions\">",
            "    <!-- ======================== Marker Styles. Automatically generated together with an enumeration for the marker styles. ======================== -->",
            "    <local:MarkerStyleList x:Key=\"MarkerStyles\">"
        };
        // ReSharper disable InconsistentNaming
        // ReSharper disable ConvertToLambdaExpression
        // ReSharper disable ImplicitlyCapturedClosure
        Action<StyleColor, string, string> ArrowUp = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) Arrow Up -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"1\" HorizontalShift=\"0.5\" VerticalShift=\"0\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,1000,1000\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add("                                <LinearGradientBrush StartPoint=\"0.5,1\" EndPoint=\"0.5,0\" ColorInterpolationMode=\"ScRgbLinearInterpolation\" SpreadMethod=\"Pad\" MappingMode=\"RelativeToBoundingBox\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                    <LinearGradientBrush.GradientStops>");
            list.Add($"                                        <GradientStop Color=\"#40{hex}\" Offset=\"0\" />");
            list.Add($"                                        <GradientStop Color=\"#B0{hex}\" Offset=\"0.53\" />");
            list.Add($"                                        <GradientStop Color=\"#FE{hex}\" Offset=\"1\" />");
            list.Add("                                    </LinearGradientBrush.GradientStops>");
            list.Add("                                </LinearGradientBrush>");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!--");
            list.Add("                                    a = width (1000)");
            list.Add("                                    M a/2,0 L 0,a/2 L a/4,a/2 L a/4,a L 3a/4,a L 3a/4,a/2 L a,a/2 L a/2,0");
            list.Add("                                -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M500,0L0,500H250V1000H750V500H1000z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                         </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        Action<StyleColor, string, string> ArrowDown = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) Arrow Down -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"1\" HorizontalShift=\"0.5\" VerticalShift=\"1\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,1000,1000\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add("                                <LinearGradientBrush StartPoint=\"0.5,0\" EndPoint=\"0.5,1\" ColorInterpolationMode=\"ScRgbLinearInterpolation\" SpreadMethod=\"Pad\" MappingMode=\"RelativeToBoundingBox\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                    <LinearGradientBrush.GradientStops>");
            list.Add($"                                        <GradientStop Color=\"#40{hex}\" Offset=\"0\" />");
            list.Add($"                                        <GradientStop Color=\"#B0{hex}\" Offset=\"0.53\" />");
            list.Add($"                                        <GradientStop Color=\"#FE{hex}\" Offset=\"1\" />");
            list.Add("                                    </LinearGradientBrush.GradientStops>");
            list.Add("                                </LinearGradientBrush>");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!--");
            list.Add("                                    a = width (1000)");
            list.Add("                                    M a/2,a L a,a/2 L 3a/4,a/2 L 3a/4,0 L a/4,0 L a/4,a/2 L 0,a/2 L a/2,a");
            list.Add("                                -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M500,1000L1000,500H750V0H250V500H0z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                         </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        Action<StyleColor, string, string> ArrowRight = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) Arrow Right -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"1\" HorizontalShift=\"1\" VerticalShift=\"0.5\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,1000,1000\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add("                                <LinearGradientBrush StartPoint=\"0,0.5\" EndPoint=\"1,0.5\" ColorInterpolationMode=\"ScRgbLinearInterpolation\" SpreadMethod=\"Pad\" MappingMode=\"RelativeToBoundingBox\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                    <LinearGradientBrush.GradientStops>");
            list.Add($"                                        <GradientStop Color=\"#40{hex}\" Offset=\"0\" />");
            list.Add($"                                        <GradientStop Color=\"#B0{hex}\" Offset=\"0.53\" />");
            list.Add($"                                        <GradientStop Color=\"#FE{hex}\" Offset=\"1\" />");
            list.Add("                                    </LinearGradientBrush.GradientStops>");
            list.Add("                                </LinearGradientBrush>");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!--");
            list.Add("                                    a = width (1000)");
            list.Add("                                    M a,a/2 L a/2,a L a/2,3a/4 L 0,3a/4 L 0,a/4 L a/2,a/4 L a/2,0 L a,a/2");
            list.Add("                                -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M1000,500L500,1000V750H0V250H500V0z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                         </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        Action<StyleColor, string, string> ArrowLeft = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) Arrow Left -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"1\" HorizontalShift=\"0\" VerticalShift=\"0.5\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,1000,1000\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add("                                <LinearGradientBrush StartPoint=\"1,0.5\" EndPoint=\"0,0.5\" ColorInterpolationMode=\"ScRgbLinearInterpolation\" SpreadMethod=\"Pad\" MappingMode=\"RelativeToBoundingBox\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                    <LinearGradientBrush.GradientStops>");
            list.Add($"                                        <GradientStop Color=\"#40{hex}\" Offset=\"0\" />");
            list.Add($"                                        <GradientStop Color=\"#B0{hex}\" Offset=\"0.53\" />");
            list.Add($"                                        <GradientStop Color=\"#FE{hex}\" Offset=\"1\" />");
            list.Add("                                    </LinearGradientBrush.GradientStops>");
            list.Add("                                </LinearGradientBrush>");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!--");
            list.Add("                                    a = width (1000)");
            list.Add("                                    M 0,a/2 L a/2,0 L a/2,a/4 L a,a/4 L a,3a/4 L a/2,3a/4 L a/2,a L 0,a/2");
            list.Add("                                -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M0,500L500,0V250H1000V750H500V1000z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                         </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        Action<StyleColor, string, string> ArrowRightUp = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) Arrow RightUp -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"1\" HorizontalShift=\"1\" VerticalShift=\"0\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,1000,1000\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add("                                <LinearGradientBrush StartPoint=\"0,1\" EndPoint=\"1,0\" ColorInterpolationMode=\"ScRgbLinearInterpolation\" SpreadMethod=\"Pad\" MappingMode=\"RelativeToBoundingBox\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                    <LinearGradientBrush.GradientStops>");
            list.Add($"                                        <GradientStop Color=\"#40{hex}\" Offset=\"0\" />");
            list.Add($"                                        <GradientStop Color=\"#B0{hex}\" Offset=\"0.53\" />");
            list.Add($"                                        <GradientStop Color=\"#FE{hex}\" Offset=\"1\" />");
            list.Add("                                    </LinearGradientBrush.GradientStops>");
            list.Add("                                </LinearGradientBrush>");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!--");
            list.Add("                                    a = width (1000)");
            list.Add("                                    M a,0 L a,a/√2 L a-a/4√2,3a/4√2 L a-3a/4√2,5a/4√2 L a-5a/4√2,3a/4√2 L a-3a/4√2,a/4√2 L a-a/√2,0 L a,0");
            list.Add("                                -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M1000,0V707.10678118654752440084436210485L823.22330470336311889978890947379,530.33008588991064330063327157864L469.66991411008935669936672842136,883.88347648318440550105545263106L116.11652351681559449894454736894,530.33008588991064330063327157864L469.66991411008935669936672842136,176.77669529663688110021109052621L292.89321881345247559915563789515,0z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                         </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        Action<StyleColor, string, string> ArrowLeftUp = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) Arrow LeftUp -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"1\" HorizontalShift=\"0\" VerticalShift=\"0\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,1000,1000\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add("                                <LinearGradientBrush StartPoint=\"1,1\" EndPoint=\"0,0\" ColorInterpolationMode=\"ScRgbLinearInterpolation\" SpreadMethod=\"Pad\" MappingMode=\"RelativeToBoundingBox\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                    <LinearGradientBrush.GradientStops>");
            list.Add($"                                        <GradientStop Color=\"#40{hex}\" Offset=\"0\" />");
            list.Add($"                                        <GradientStop Color=\"#B0{hex}\" Offset=\"0.53\" />");
            list.Add($"                                        <GradientStop Color=\"#FE{hex}\" Offset=\"1\" />");
            list.Add("                                    </LinearGradientBrush.GradientStops>");
            list.Add("                                </LinearGradientBrush>");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!--");
            list.Add("                                    a = width (1000)");
            list.Add("                                    M 0,0 L a/√2,0 L 3a/4√2,a/4√2 L 5a/4√2,3a/4√2 L 3a/4√2,5a/4√2 L a/4√2,3a/4√2 L 0,a/√2 L 0,0");
            list.Add("                                -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M0,0H707.10678118654752440084436210485L530.33008588991064330063327157864,176.77669529663688110021109052621L883.88347648318440550105545263106,530.33008588991064330063327157864L530.33008588991064330063327157864,883.88347648318440550105545263106L176.77669529663688110021109052621,530.33008588991064330063327157864L0,707.10678118654752440084436210485z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                         </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        Action<StyleColor, string, string> ArrowLeftDown = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) Arrow LeftDown -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"1\" HorizontalShift=\"0\" VerticalShift=\"1\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,1000,1000\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add("                                <LinearGradientBrush StartPoint=\"1,0\" EndPoint=\"0,1\" ColorInterpolationMode=\"ScRgbLinearInterpolation\" SpreadMethod=\"Pad\" MappingMode=\"RelativeToBoundingBox\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                    <LinearGradientBrush.GradientStops>");
            list.Add($"                                        <GradientStop Color=\"#40{hex}\" Offset=\"0\" />");
            list.Add($"                                        <GradientStop Color=\"#B0{hex}\" Offset=\"0.53\" />");
            list.Add($"                                        <GradientStop Color=\"#FE{hex}\" Offset=\"1\" />");
            list.Add("                                    </LinearGradientBrush.GradientStops>");
            list.Add("                                </LinearGradientBrush>");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!--");
            list.Add("                                    a = width (1000)");
            list.Add("                                    M 0,a L a/√2,a L 3a/4√2,a-a/4√2 L 5a/4√2,a-3a/4√2 L 3a/4√2,a-5a/4√2 L a/4√2,a-3a/4√2 L 0,a-a/√2 L 0,a");
            list.Add("                                -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M0,1000H707.10678118654752440084436210485L530.33008588991064330063327157864,823.22330470336311889978890947379L883.88347648318440550105545263106,469.66991411008935669936672842136L530.33008588991064330063327157864,116.11652351681559449894454736894L176.77669529663688110021109052621,469.66991411008935669936672842136L0,292.89321881345247559915563789515z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                         </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        Action<StyleColor, string, string> ArrowRightDown = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) Arrow RightDown -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"1\" HorizontalShift=\"1\" VerticalShift=\"1\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,1000,1000\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <DrawingBrush.Drawing>");
            list.Add("                        <GeometryDrawing PresentationOptions:Freeze=\"True\">");
            list.Add("                            <GeometryDrawing.Brush>");
            list.Add("                                <LinearGradientBrush StartPoint=\"0,0\" EndPoint=\"1,1\" ColorInterpolationMode=\"ScRgbLinearInterpolation\" SpreadMethod=\"Pad\" MappingMode=\"RelativeToBoundingBox\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                    <LinearGradientBrush.GradientStops>");
            list.Add($"                                        <GradientStop Color=\"#40{hex}\" Offset=\"0\" />");
            list.Add($"                                        <GradientStop Color=\"#B0{hex}\" Offset=\"0.53\" />");
            list.Add($"                                        <GradientStop Color=\"#FE{hex}\" Offset=\"1\" />");
            list.Add("                                    </LinearGradientBrush.GradientStops>");
            list.Add("                                </LinearGradientBrush>");
            list.Add("                            </GeometryDrawing.Brush>");
            list.Add("                            <GeometryDrawing.Geometry>");
            list.Add("                                <!--");
            list.Add("                                    a = width (1000)");
            list.Add("                                    M a,a L a,a-a/√2 L a-a/4√2,a-3a/4√2 L a-3a/4√2,a-5a/4√2 L a-5a/4√2,a-3a/4√2 L a-3a/4√2,a-a/4√2 L a-a/√2,a L a,a");
            list.Add("                                -->");
            list.Add("                                <PathGeometry FillRule=\"Nonzero\" Figures=\"M1000,1000V292.89321881345247559915563789515L823.22330470336311889978890947379,469.66991411008935669936672842136L469.66991411008935669936672842136,116.11652351681559449894454736894L116.11652351681559449894454736894,469.66991411008935669936672842136L469.66991411008935669936672842136,823.22330470336311889978890947379L292.89321881345247559915563789515,1000z\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                            </GeometryDrawing.Geometry>");
            list.Add("                         </GeometryDrawing>");
            list.Add("                    </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        Action<StyleColor, string, string> CalloutBox = (v, e, d) =>
        {
            string hex = v.Color.Substring(1);
            list.Add($"         <!-- {v.Name} ({v.Color}) CalloutBox -->");
            list.Add($"         <local:MarkerStyle Enumeration=\"{e}\" MinimalWidth=\"10\" MaximalWidth=\"300\" HeightToWidthRatio=\"0.9314127316913342\" HorizontalShift=\"0.9\" VerticalShift=\"1\">");
            list.Add("             <local:MarkerStyle.Brush>");
            list.Add("                 <DrawingBrush ViewboxUnits=\"Absolute\" Viewbox=\"0,0,497.483,463.362\" Stretch=\"Uniform\" PresentationOptions:Freeze=\"True\">");
            list.Add("                     <DrawingBrush.Drawing>");
            list.Add("                             <DrawingGroup Transform=\"0.999999812532888,0,0,1.0000003303599,0,0\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                 <DrawingGroup.Children>");
            list.Add($"                                     <GeometryDrawing Brush=\"#50{hex}\" Geometry=\"M76.7452,28.737L413.705,31.1353C434.518,31.2834 469.761,36.6093 477.259,65.9102 485.92,104.443 488.806,277.858 474.86,304.541 464.579,326.062 455.032,333.239 444.883,339.315 414.818,353.758 274.603,351.306 274.603,351.306L406.509,452.035 211.049,350.107 65.9528,350.108C45.5927,350.108 26.9741,337.304 20.3855,309.336 8.91394,258.553 6.68087,116.123 17.9871,82.6982 28.7013,52.1598 46.7079,28.5232 76.7452,28.737z\" PresentationOptions:Freeze=\"True\">");
            list.Add("                                         <GeometryDrawing.Pen>");
            list.Add("                                             <Pen Brush=\"#50000000\" Thickness=\"10\" StartLineCap=\"Flat\" EndLineCap=\"Flat\" DashCap=\"Flat\" LineJoin=\"Round\" MiterLimit=\"4\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                                         </GeometryDrawing.Pen>");
            list.Add("                                     </GeometryDrawing>");
            list.Add("                                 </DrawingGroup.Children>");
            list.Add("                             </DrawingGroup>");
            list.Add("                     </DrawingBrush.Drawing>");
            list.Add("                 </DrawingBrush>");
            list.Add("            </local:MarkerStyle.Brush>");
            list.Add("         </local:MarkerStyle>");
        };
        // ReSharper restore ImplicitlyCapturedClosure
        // ReSharper restore ConvertToLambdaExpression
        // ReSharper restore InconsistentNaming

        int i = 0;
        foreach (var v in enumerable)
        {
            string e = listEnumeration[i];
            string d = listDescription[i];
            ++i;
            ArrowUp(v, e, d);
            e = listEnumeration[i];
            d = listDescription[i];
            ++i;
            ArrowDown(v, e, d);
            e = listEnumeration[i];
            d = listDescription[i];
            ++i;
            ArrowRight(v, e, d);
            e = listEnumeration[i];
            d = listDescription[i];
            ++i;
            ArrowLeft(v, e, d);
            e = listEnumeration[i];
            d = listDescription[i];
            ++i;
            ArrowRightUp(v, e, d);
            e = listEnumeration[i];
            d = listDescription[i];
            ++i;
            ArrowLeftUp(v, e, d);
            e = listEnumeration[i];
            d = listDescription[i];
            ++i;
            ArrowLeftDown(v, e, d);
            e = listEnumeration[i];
            d = listDescription[i];
            ++i;
            ArrowRightDown(v, e, d);
        }
        foreach (var v in enumerable)
        {
            string e = listEnumeration[i];
            string d = listDescription[i];
            ++i;
            CalloutBox(v, e, d);
        }
        list.Add("    </local:MarkerStyleList>");
        list.Add("</ResourceDictionary>");
        return list;
    }

    private static IEnumerable<string> GenerateMarkerStylesEnum(StyleColor[] enumerable, List<string> listEnumeration, List<string> listDescription)
    {
        listEnumeration.Clear();
        listDescription.Clear();
        bool first = true;
        var list = new List<string>(4096)
        {
            "namespace Mbst.Charts",
            "{",
            "    /// <summary>",
            "    /// Enumerates predefined marker styles.",
            "    /// </summary>",
            "    public enum PredefinedMarkerStyle",
            "    {",
            "        // This code is automatically generated together with a XAML code for the marker styles."
        };
        foreach (var v in enumerable)
        {
            list.Add("");
            string enumeration = string.Concat(v.Name, "ArrowUp");
            string description = string.Concat(v.Name, " Arrow Up.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            if (first)
            {
                first = false;
                list.Add($"        {enumeration} = 0,");
            }
            else
                list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "ArrowDown");
            description = string.Concat(v.Name, " Arrow Down.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "ArrowRight");
            description = string.Concat(v.Name, " Arrow Right.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "ArrowLeft");
            description = string.Concat(v.Name, " Arrow Left.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "ArrowRightUp");
            description = string.Concat(v.Name, " Arrow Right Up.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "ArrowLeftUp");
            description = string.Concat(v.Name, " Arrow Left Up.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "ArrowLeftDown");
            description = string.Concat(v.Name, " Arrow Left Down.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "ArrowRightDown");
            description = string.Concat(v.Name, " Arrow Right Down.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
        }
        foreach (var v in enumerable)
        {
            list.Add("");
            string enumeration = string.Concat(v.Name, "CalloutBox");
            string description = string.Concat(v.Name, " Callout Box.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
        }
        list.Add("    }");
        list.Add("}");
        return list;
    }
    #endregion

    #region Function styles
    private static IEnumerable<string> GenerateFunctionStylesXaml(IEnumerable<StyleColor> enumerable, List<string> listEnumeration, List<string> listDescription)
    {
        var list = new List<string>(4096)
        {
            "<ResourceDictionary",
            "    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
            "    xmlns:PresentationOptions=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation/options\"",
            "    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
            "    xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
            "    xmlns:local=\"clr-namespace:Mbst.Charts\"",
            "    mc:Ignorable=\"PresentationOptions\">",
            "    <!-- ======================== Function Styles. Automatically generated together with an enumeration for the function styles. ======================== -->",
            "    <local:FunctionStyleList x:Key=\"FunctionStyles\">"
        };
        int i = 0;
        foreach (var v in enumerable)
        {
            list.Add($"        <!-- {v.Name} {v.Color} -->");
            string e = listEnumeration[i];
            // ReSharper disable once NotAccessedVariable
            string d = listDescription[i];
            ++i;
            list.Add($"        <local:FunctionStyle Enumeration=\"{e}\">");
            list.Add("            <local:FunctionStyle.Pen>");
            list.Add("                <Pen Thickness=\"0.9\" DashStyle=\"{x:Static DashStyles.Solid}\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <Pen.Brush>");
            list.Add($"                        <SolidColorBrush Color=\"{v.Color}\" Opacity=\"0.8\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                    </Pen.Brush>");
            list.Add("                </Pen>");
            list.Add("            </local:FunctionStyle.Pen>");
            list.Add("        </local:FunctionStyle>");

            e = listEnumeration[i];
            // ReSharper disable once RedundantAssignment
            d = listDescription[i];
            ++i;
            list.Add($"        <local:FunctionStyle Enumeration=\"{e}\">");
            list.Add("            <local:FunctionStyle.Pen>");
            list.Add("                <Pen Thickness=\"0.9\" DashStyle=\"{x:Static DashStyles.Solid}\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <Pen.Brush>");
            list.Add($"                        <SolidColorBrush Color=\"{v.Color}\" Opacity=\"0.9\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                    </Pen.Brush>");
            list.Add("                </Pen>");
            list.Add("            </local:FunctionStyle.Pen>");
            list.Add("            <local:FunctionStyle.Fill>");
            list.Add($"                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"0.4\" PresentationOptions:Freeze=\"True\" />");
            list.Add("            </local:FunctionStyle.Fill>");
            list.Add("        </local:FunctionStyle>");

            e = listEnumeration[i];
            // ReSharper disable once RedundantAssignment
            d = listDescription[i];
            ++i;
            list.Add($"        <local:FunctionStyle Enumeration=\"{e}\">");
            list.Add("            <local:FunctionStyle.Fill>");
            list.Add($"                <SolidColorBrush Color=\"{v.Color}\" Opacity=\"0.4\" PresentationOptions:Freeze=\"True\" />");
            list.Add("            </local:FunctionStyle.Fill>");
            list.Add("        </local:FunctionStyle>");
        }
        list.Add("    </local:FunctionStyleList>");
        list.Add("</ResourceDictionary>");
        return list;
    }

    private static IEnumerable<string> GenerateFunctionStylesEnum(IEnumerable<StyleColor> enumerable, List<string> listEnumeration, List<string> listDescription)
    {
        listEnumeration.Clear();
        listDescription.Clear();
        bool first = true;
        var list = new List<string>(8192)
        {
            "namespace Mbst.Charts",
            "{",
            "    /// <summary>",
            "    /// Enumerates predefined function styles.",
            "    /// </summary>",
            "    public enum PredefinedFunctionStyle",
            "    {",
            "        // This code is automatically generated together with a XAML code for the function styles.", ""
        };
        foreach (var v in enumerable)
        {
            string enumeration = string.Concat(v.Name, "Line");
            string description = string.Concat("A ", v.Name, " line without an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            if (first)
            {
                first = false;
                list.Add($"        {enumeration} = 0,");
            }
            else
                list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "LineFill");
            description = string.Concat("A ", v.Name, " line with an interior fill.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "Fill");
            description = string.Concat("An ", v.Name, " interior fill without a bounding line.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
        }
        list.Add("    }");
        list.Add("}");
        return list;
    }
    #endregion

    #region Guideline styles
    private static IEnumerable<string> GenerateGuideLineStylesXaml(IEnumerable<StyleColor> enumerable, List<string> listEnumeration, List<string> listDescription)
    {
        var list = new List<string>(8192)
        {
            "<ResourceDictionary",
            "    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
            "    xmlns:PresentationOptions=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation/options\"",
            "    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
            "    xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
            "    xmlns:local=\"clr-namespace:Mbst.Charts\"",
            "    mc:Ignorable=\"PresentationOptions\">",
            "    <!-- ======================== GuideLine Styles. Automatically generated together with an enumeration for the guideline styles. ======================== -->",
            "    <local:GuideLineStyleList x:Key=\"GuideLineStyles\">"
        };
        int i = 0;
        foreach (var v in enumerable)
        {
            list.Add($"        <!-- {v.Name} {v.Color} -->");
            string e = listEnumeration[i];
            // ReSharper disable once NotAccessedVariable
            string d = listDescription[i];
            ++i;
            list.Add($"        <local:GuideLineStyle Enumeration=\"{e}\">");
            list.Add("            <local:GuideLineStyle.Pen>");
            list.Add("                <Pen Thickness=\"0.9\" DashStyle=\"{x:Static DashStyles.Solid}\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <Pen.Brush>");
            list.Add($"                        <SolidColorBrush Color=\"{v.Color}\" Opacity=\"0.6\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                    </Pen.Brush>");
            list.Add("                </Pen>");
            list.Add("            </local:GuideLineStyle.Pen>");
            list.Add("        </local:GuideLineStyle>");

            e = listEnumeration[i];
            // ReSharper disable once RedundantAssignment
            d = listDescription[i];
            ++i;
            list.Add($"        <local:GuideLineStyle Enumeration=\"{e}\">");
            list.Add("            <local:GuideLineStyle.Pen>");
            list.Add("                <Pen Thickness=\"0.9\" DashStyle=\"{x:Static DashStyles.Dot}\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <Pen.Brush>");
            list.Add($"                        <SolidColorBrush Color=\"{v.Color}\" Opacity=\"0.6\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                    </Pen.Brush>");
            list.Add("                </Pen>");
            list.Add("            </local:GuideLineStyle.Pen>");
            list.Add("        </local:GuideLineStyle>");

            e = listEnumeration[i];
            // ReSharper disable once RedundantAssignment
            d = listDescription[i];
            ++i;
            list.Add($"        <local:GuideLineStyle Enumeration=\"{e}\">");
            list.Add("            <local:GuideLineStyle.Pen>");
            list.Add("                <Pen Thickness=\"0.9\" DashStyle=\"{x:Static DashStyles.Dash}\" PresentationOptions:Freeze=\"True\">");
            list.Add("                    <Pen.Brush>");
            list.Add($"                        <SolidColorBrush Color=\"{v.Color}\" Opacity=\"0.6\" PresentationOptions:Freeze=\"True\" />");
            list.Add("                    </Pen.Brush>");
            list.Add("                </Pen>");
            list.Add("            </local:GuideLineStyle.Pen>");
            list.Add("        </local:GuideLineStyle>");
        }
        list.Add("    </local:GuideLineStyleList>");
        list.Add("</ResourceDictionary>");
        return list;
    }

    private static IEnumerable<string> GenerateGuideLineStylesEnum(IEnumerable<StyleColor> enumerable, List<string> listEnumeration, List<string> listDescription)
    {
        listEnumeration.Clear();
        listDescription.Clear();
        bool first = true;
        var list = new List<string>(8192)
        {
            "namespace Mbst.Charts",
            "{",
            "    /// <summary>",
            "    /// Enumerates predefined guide line styles.",
            "    /// </summary>",
            "    public enum PredefinedGuideLineStyle",
            "    {",
            "        // This code is automatically generated together with a XAML code for the guideline styles.",
            ""
        };
        foreach (var v in enumerable)
        {
            string enumeration = string.Concat(v.Name, "Solid");
            string description = string.Concat("A ", v.Name, " solid line.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            if (first)
            {
                first = false;
                list.Add($"        {enumeration} = 0,");
            }
            else
                list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "Dotted");
            description = string.Concat("A ", v.Name, " dotted line.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
            enumeration = string.Concat(v.Name, "Dashed");
            description = string.Concat("A ", v.Name, " dashed line.");
            listEnumeration.Add(enumeration);
            listDescription.Add(description);
            list.Add("        /// <summary>");
            list.Add($"        /// {description}");
            list.Add("        /// </summary>");
            list.Add($"        {enumeration},");
            list.Add("");
        }
        list.Add("    }");
        list.Add("}");
        return list;
    }
    #endregion
}