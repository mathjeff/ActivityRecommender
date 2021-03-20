using VisiPlacement;

namespace ActivityRecommendation
{
    // A set of contributors that can be referenced by various other screens
    // These are compile-time constants so their existence can be validated at compile time
    public class ActRecContributor
    {
        // sorted in alphabetical order
        public static AppContributor AARON_SMITH = new AppContributor("Aaron Smith");
        public static AppContributor ANNI_ZHANG = new AppContributor("Anni Zhang", "anniz44@mit.edu", "https://web.mit.edu/almlab/anni_zhang.html");
        public static AppContributor CORY_JALBERT = new AppContributor("Cory Jalbert");
        public static AppContributor DAD = new AppContributor("Dad");
        public static AppContributor JEFFRY_GASTON = new AppContributor("Jeff Gaston", null, "github.com/mathjeff");
        public static AppContributor TOBY_HUANG = new AppContributor("Toby Huang", "tobyqrhuang93@gmail.com");
        public static AppContributor TONY_FISCHETTI = new AppContributor("Tony Fischetti", null, "tonyfischettiart.com");
    }
}
