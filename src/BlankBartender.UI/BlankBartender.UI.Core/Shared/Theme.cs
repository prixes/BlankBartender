using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankBartender.UI.Core.Shared
{
    using MudBlazor;

    public static class Theme
    {
        public static MudTheme AppTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#5E39BA", // Example custom primary color
                Secondary = "#9C27B0", // Example custom secondary color
                Success = "#4CAF50",
                Error = "#F44336",
                Background = "#F5F5F5",
                Surface = "#00000000",
                AppbarBackground = "#FFFFFF",
                TextPrimary = "#F4F4F4",
                TextSecondary = "#9B95A5",
                Tertiary = "#90E47B",
                Dark = "#5E39BA"
                // Add other colors as needed
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#5E39BA",
                Secondary = "#03DAC6",
                Background = "#121212",
                AppbarBackground = "#1E1E1E",
                Surface = "#00000000",
                DrawerBackground = "#272727",
                TextPrimary = "#F4F4F4",
                TextSecondary = "#9B95A5",
                Tertiary = "#90E47B",
                Dark = "#5E39BA"
                // Customize dark mode colors
            },
            Typography = new Typography()
            {
                Body1 = new Body1
                {
                    FontFamily = new[] { "Koulen", "Arial", "sans-serif" },
                    FontSize = "13px",
                },
                H5 = new H5()
                {
                    FontFamily = ["Istok Web", "Arial", "sans-serif"],
                },
                H6 = new H6()
                {
                    FontFamily = ["Koulen", "Arial", "sans-serif"],
                    FontWeight = 400,
                    FontSize = "16px",
                    LetterSpacing = "-0.5px",
                }
                // Add more for H3, H4, Body1, Body2, etc., as needed.

            }

        };
    }
}
