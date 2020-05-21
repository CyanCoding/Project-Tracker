Natively WPF windows can't process `.svg` files. Therefore, we need to
change how we process them to display them in the Project Tracker.
We do this by using [SvgToXaml](https://github.com/BerndK/SvgToXaml).
This tool allows us to generate a `.xaml` file from `.svg`s to display
on a canvas in our program. Here you can find all of the currently
used `.svg` files and the `.xaml` file with the information for all
of them.
