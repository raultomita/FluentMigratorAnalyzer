using System;
using System.Collections.Generic;
using System.Text;

namespace FluentMigratorAnalyzer
{
    class ClassNames
    {
        internal static string Migration = "Migration";
        internal static string MigrationUp = "MigrationUp";
        internal static string Profile = "Profile";

        internal static List<string> All = new List<string> { Migration, MigrationUp, Profile };

    }
}
