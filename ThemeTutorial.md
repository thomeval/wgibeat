# Introduction #

This page contains an explanation on how themes work in WGiBeat. A theme is a collection of graphics that change the appearance of WGiBeat when used. Note that theme support is only available in version 0.6 or later.


# Basics #

All WGiBeat themes are stored in the Content\Textures subfolder. The only theme included with WGiBeat is the 'Default' theme, which will be located in the Content\Textures\Default folder. Each theme folder contains the necessary graphics. Although it is possible to simply edit these graphics, it is recommended to create custom theme instead. To do this:

  * Create a new folder inside the Textures subfolder (next to the Default folder). The folder can have any name.
  * Copy any graphics that will be replaced to this new subfolder.
  * (Optional) Copy the metrics.txt file to the new subfolder, or create a new one there.
  * Edit the copied graphics using the method and program of your choice.
  * Run WGiBeat, and change the active theme from the Options Screen.

# Metrics #

The Default theme also contains a metrics.txt file, which defines the position of graphics on the screen. This can be edited using any text editor. The format of the metrics file is as follows:

```
#METRICS-1.0;
ItemName1=[x,y][x,y];
ItemName2=[x,y];
etc.
```

  * The file must begin with #METRICS-1.0; on the first line.
  * Every line must end with a semicolon.
  * Any other line beginning with a '#' is ignored (however, they must also end in a semicolon).
  * Metric definitions consist of a name, and `[x,y]` coordinate pairs. The number of coordinate pairs vary depending on how many are needed. For example, there are four pairs for defining the position of each player's score display.
  * The names are hard-coded and must be correct to be recognized by WGiBeat. They should also have the correct case.

# Notes #

  * Graphic file names are not case sensitive, but files must have the correct names to be recognized by WGiBeat.
  * Theme graphics can be  in .bmp, .jpg or .png format.
  * Any arrangement of subfolders are permitted inside a particular theme folder. WGiBeat will automatically scan through all subfolders to find theme graphics.
  * If any graphics are not found inside a particular theme folder, the corresponding graphic from the Default theme will be used instead. Use this to your advantage by only including graphics that should be different in custom theme folders.
  * The same also applies to metrics. Any metrics not defined in a custom theme's metrics.txt file (or if it doesn't have one) will be taken from the Default theme instead.