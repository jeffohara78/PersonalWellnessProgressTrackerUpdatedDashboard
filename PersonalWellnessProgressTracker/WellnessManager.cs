using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PersonalWellnessProgressTracker
{
    public class WellnessManager
    {
        // UPDATED 06/11/2026:
        // Replaced one single profile with a list of profiles.
        private List<UserProfile> profiles = new List<UserProfile>();

        private int nextProfileId = 101;

        private List<ProgressEntry> progressEntries = new List<ProgressEntry>();

        private List<WorkoutEntry> workoutEntries = new List<WorkoutEntry>();

        private int nextProgressId = 1001;

        private int nextWorkoutId = 5001;

        // UPDATED 06/11/2026:
        // Stores multiple user profiles instead of one single profile.
        private string profileFilePath = "userProfiles.json";

        private string progressFilePath = "progressEntries.json";

        private string workoutFilePath = "workoutEntries.json";

        public WellnessManager()
        {
            LoadDataFromFiles();
        }

        // UPDATED 06/11/2026:
        // This method now creates a new profile instead of replacing one single profile.
        public void CreateOrUpdateProfile()
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("             ADD PROFILE");
            Console.WriteLine("======================================");
            Console.WriteLine("Use this screen to add a person who wants");
            Console.WriteLine("to track weight, wellness progress, workouts,");
            Console.WriteLine("BMI, and optional body fat percentage.");
            Console.WriteLine();
            Console.WriteLine("Each person receives their own profile ID,");
            Console.WriteLine("so multiple people can track progress separately.");
            Console.WriteLine();
            Console.WriteLine("Enter 0 at any prompt to cancel without saving.");
            Console.WriteLine();

            string fullName = GetTextOrCancel("Name: ");

            if (fullName == "0")
            {
                Console.WriteLine("Profile creation cancelled.");
                return;
            }

            decimal height = GetDecimalOrCancel("Height in inches, such as 73 for 6'1\", or 0 to cancel: ");

            if (height == 0)
            {
                Console.WriteLine("Profile creation cancelled.");
                return;
            }

            decimal startingWeight = GetDecimalOrCancel("Starting weight in pounds, or 0 to cancel: ");

            if (startingWeight == 0)
            {
                Console.WriteLine("Profile creation cancelled.");
                return;
            }

            decimal goalWeight = GetDecimalOrCancel("Goal weight in pounds, or 0 to cancel: ");

            if (goalWeight == 0)
            {
                Console.WriteLine("Profile creation cancelled.");
                return;
            }

            UserProfile profile = new UserProfile(
                nextProfileId,
                fullName,
                height,
                startingWeight,
                goalWeight
            );

            profiles.Add(profile);

            SaveDataToFiles();

            Console.WriteLine("\nProfile saved successfully.");
            Console.WriteLine($"Profile ID: {nextProfileId}");
            Console.WriteLine($"Name: {fullName}");

            nextProfileId++;
        }

        // UPDATED 06/11/2026:
        // Allows the user to choose which profile to view.
        public void ViewProfile()
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("              VIEW PROFILE");
            Console.WriteLine("======================================");

            if (profiles.Count == 0)
            {
                Console.WriteLine("No profiles have been created yet.");
                return;
            }

            DisplayProfileSummary();

            int profileId = GetIntOrCancel("\nEnter Profile ID to view, or 0 to cancel: ");

            if (profileId == 0)
            {
                Console.WriteLine("View profile cancelled.");
                return;
            }

            UserProfile profile = profiles.Find(item => item.ProfileId == profileId);

            if (profile == null)
            {
                Console.WriteLine("No profile with that ID was found.");
                return;
            }

            Console.WriteLine($"\nName: {profile.FullName}");
            Console.WriteLine($"Height: {profile.HeightInInches} inches");
            Console.WriteLine($"Starting Weight: {profile.StartingWeight} lbs");
            Console.WriteLine($"Goal Weight: {profile.GoalWeight} lbs");

            decimal currentWeight = GetCurrentWeight(profile.ProfileId);

            if (currentWeight > 0)
            {
                decimal bmi = CalculateBmi(currentWeight, profile.HeightInInches);

                Console.WriteLine($"Current Weight: {currentWeight} lbs");
                Console.WriteLine($"Current BMI: {bmi:F1}");
                Console.WriteLine($"BMI Category: {GetBmiCategory(bmi)}");
            }
        }

        // UPDATED 06/11/2026:
        // Progress entries now attach to a selected profile.
        // UPDATED 06/12/2026:
        // Improved the progress entry workflow so the user understands
        // that this section tracks body measurements, not workouts.
        public void AddProgressEntry()
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("          ADD PROGRESS ENTRY");
            Console.WriteLine("======================================");
            Console.WriteLine("Use this screen to record body progress");
            Console.WriteLine("for one saved profile.");
            Console.WriteLine();
            Console.WriteLine("This includes:");
            Console.WriteLine("- Current weight");
            Console.WriteLine("- Optional body fat percentage");
            Console.WriteLine("- Notes about progress, diet, energy, or changes");
            Console.WriteLine();
            Console.WriteLine("This does not record workout activity.");
            Console.WriteLine("Workout activity is added separately using");
            Console.WriteLine("the Add Workout Entry option.");
            Console.WriteLine();
            Console.WriteLine("Both progress entries and workout entries");
            Console.WriteLine("will appear together in the wellness dashboard.");
            Console.WriteLine();
            Console.WriteLine("Enter 0 at any prompt to cancel without saving.");
            Console.WriteLine();

            if (profiles.Count == 0)
            {
                Console.WriteLine("Create a profile before adding progress entries.");
                return;
            }

            DisplayProfileSummary();

            int profileId = GetIntOrCancel("\nEnter Profile ID for this progress entry, or 0 to cancel: ");

            if (profileId == 0)
            {
                Console.WriteLine("Progress entry cancelled.");
                return;
            }

            UserProfile profile = profiles.Find(item => item.ProfileId == profileId);

            if (profile == null)
            {
                Console.WriteLine("No profile with that ID was found.");
                return;
            }

            decimal weight = GetDecimalOrCancel("Current weight in pounds, or 0 to cancel: ");

            if (weight == 0)
            {
                Console.WriteLine("Progress entry cancelled.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Body fat percentage is optional.");
            Console.WriteLine("It may come from a smart scale, trainer,");
            Console.WriteLine("doctor, fitness assessment, or body measurement calculator.");
            Console.WriteLine("If you do not know it, press ENTER to skip.");
            Console.WriteLine();

            Console.Write("Body fat percentage, or press ENTER to skip: ");
            string bodyFatInput = Console.ReadLine().Trim();

            decimal? bodyFatPercentage = null;

            if (!string.IsNullOrWhiteSpace(bodyFatInput))
            {
                bool validBodyFat = decimal.TryParse(bodyFatInput, out decimal bodyFat);

                if (validBodyFat && bodyFat >= 0)
                {
                    bodyFatPercentage = bodyFat;
                }
                else
                {
                    Console.WriteLine("Invalid body fat percentage. Entry cancelled.");
                    return;
                }
            }

            string notes = GetTextOrCancel("Progress notes, or press ENTER to leave blank: ");

            if (notes == "0")
            {
                Console.WriteLine("Progress entry cancelled.");
                return;
            }

            ProgressEntry entry = new ProgressEntry(
                nextProgressId,
                profileId,
                weight,
                bodyFatPercentage,
                notes
            );

            progressEntries.Add(entry);

            SaveDataToFiles();

            Console.WriteLine("\nProgress entry saved successfully.");
            Console.WriteLine($"Profile: {profile.FullName}");
            Console.WriteLine($"Entry ID: {nextProgressId}");
            Console.WriteLine($"Weight: {weight} lbs");

            nextProgressId++;
        }

        // UPDATED 06/11/2026:
        // Workout entries now attach to a selected profile.
        // UPDATED 06/12/2026:
        // Improved workout entry workflow so the user understands
        // that workouts are connected to the selected profile dashboard.
        public void AddWorkoutEntry()
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("           ADD WORKOUT ENTRY");
            Console.WriteLine("======================================");
            Console.WriteLine("Use this screen to record physical activity");
            Console.WriteLine("for one saved profile.");
            Console.WriteLine();
            Console.WriteLine("This includes:");
            Console.WriteLine("- Workout type");
            Console.WriteLine("- Workout length");
            Console.WriteLine("- Intensity level");
            Console.WriteLine("- Notes about how the workout went");
            Console.WriteLine();
            Console.WriteLine("This does not record body weight.");
            Console.WriteLine("Weight and body measurements are added separately");
            Console.WriteLine("using the Add Progress Entry option.");
            Console.WriteLine();
            Console.WriteLine("Both workout entries and progress entries");
            Console.WriteLine("will appear together in the wellness dashboard.");
            Console.WriteLine();
            Console.WriteLine("Enter 0 at any prompt to cancel without saving.");
            Console.WriteLine();

            if (profiles.Count == 0)
            {
                Console.WriteLine("Create a profile before adding workout entries.");
                return;
            }

            DisplayProfileSummary();

            int profileId = GetIntOrCancel("\nEnter Profile ID for this workout entry, or 0 to cancel: ");

            if (profileId == 0)
            {
                Console.WriteLine("Workout entry cancelled.");
                return;
            }

            UserProfile profile = profiles.Find(item => item.ProfileId == profileId);

            if (profile == null)
            {
                Console.WriteLine("No profile with that ID was found.");
                return;
            }

            string workoutType = GetTextOrCancel("Workout type, such as Walking, Weights, Cycling, or Stretching: ");

            if (workoutType == "0")
            {
                Console.WriteLine("Workout entry cancelled.");
                return;
            }

            int minutes = GetIntOrCancel("Workout length in minutes, or 0 to cancel: ");

            if (minutes == 0)
            {
                Console.WriteLine("Workout entry cancelled.");
                return;
            }

            string intensity = GetIntensityFromUser();

            if (intensity == "Cancel")
            {
                Console.WriteLine("Workout entry cancelled.");
                return;
            }

            string notes = GetTextOrCancel("Workout notes, or press ENTER to leave blank: ");

            if (notes == "0")
            {
                Console.WriteLine("Workout entry cancelled.");
                return;
            }

            WorkoutEntry workout = new WorkoutEntry(
                nextWorkoutId,
                profileId,
                workoutType,
                minutes,
                intensity,
                notes
            );

            workoutEntries.Add(workout);

            SaveDataToFiles();

            Console.WriteLine("\nWorkout entry saved successfully.");
            Console.WriteLine($"Profile: {profile.FullName}");
            Console.WriteLine($"Workout ID: {nextWorkoutId}");
            Console.WriteLine($"Workout: {workoutType}");
            Console.WriteLine($"Minutes: {minutes}");

            nextWorkoutId++;
        }

        public void ViewWorkoutEntries()
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("           WORKOUT ENTRIES");
            Console.WriteLine("======================================");

            if (workoutEntries.Count == 0)
            {
                Console.WriteLine("No workout entries have been added yet.");
                return;
            }

            foreach (WorkoutEntry workout in workoutEntries)
            {
                Console.WriteLine("\n------------------------------");
                Console.WriteLine($"Workout ID: {workout.WorkoutId}");
                Console.WriteLine($"Date: {workout.WorkoutDate}");
                Console.WriteLine($"Workout Type: {workout.WorkoutType}");
                Console.WriteLine($"Minutes: {workout.Minutes}");
                Console.WriteLine($"Intensity: {workout.Intensity}");
                Console.WriteLine($"Notes: {workout.Notes}");
            }
        }

        // UPDATED 06/11/2026:
        // Updated dashboard to work with multiple profiles.
        // The user now chooses which profile should be analyzed.
        // UPDATED 06/12/2026:
        // Redesigned dashboard so progress entries and workout entries
        // work together in one profile-based wellness report.
        public void ViewWellnessDashboard()
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("          WELLNESS DASHBOARD");
            Console.WriteLine("======================================");
            Console.WriteLine("This dashboard combines body progress and");
            Console.WriteLine("workout activity for one selected profile.");
            Console.WriteLine();

            if (profiles.Count == 0)
            {
                Console.WriteLine("No profiles have been created yet.");
                return;
            }

            DisplayProfileSummary();

            int profileId = GetIntOrCancel("\nEnter Profile ID to view dashboard, or 0 to cancel: ");

            if (profileId == 0)
            {
                Console.WriteLine("Dashboard view cancelled.");
                return;
            }

            UserProfile profile = profiles.Find(item => item.ProfileId == profileId);

            if (profile == null)
            {
                Console.WriteLine("No profile with that ID was found.");
                return;
            }

            decimal currentWeight = GetCurrentWeight(profile.ProfileId);
            decimal latestBodyFat = GetLatestBodyFat(profile.ProfileId);

            int totalWorkoutMinutes = 0;
            int workoutCount = 0;
            int lightWorkouts = 0;
            int moderateWorkouts = 0;
            int hardWorkouts = 0;

            foreach (WorkoutEntry workout in workoutEntries)
            {
                if (workout.ProfileId == profile.ProfileId)
                {
                    workoutCount++;
                    totalWorkoutMinutes += workout.Minutes;

                    if (workout.Intensity == "Light")
                    {
                        lightWorkouts++;
                    }
                    else if (workout.Intensity == "Moderate")
                    {
                        moderateWorkouts++;
                    }
                    else if (workout.Intensity == "Hard")
                    {
                        hardWorkouts++;
                    }
                }
            }

            int progressCount = 0;

            foreach (ProgressEntry entry in progressEntries)
            {
                if (entry.ProfileId == profile.ProfileId)
                {
                    progressCount++;
                }
            }

            Console.WriteLine("\n======================================");
            Console.WriteLine("        PROFILE WELLNESS REPORT");
            Console.WriteLine("======================================");
            Console.WriteLine($"Name: {profile.FullName}");
            Console.WriteLine($"Starting Weight: {profile.StartingWeight} lbs");
            Console.WriteLine($"Goal Weight: {profile.GoalWeight} lbs");
            Console.WriteLine();

            Console.WriteLine("--- Progress Entry Summary ---");
            Console.WriteLine($"Progress Entries: {progressCount}");

            if (currentWeight > 0)
            {
                decimal bmi = CalculateBmi(currentWeight, profile.HeightInInches);
                decimal weightChange = currentWeight - profile.StartingWeight;
                decimal poundsToGoal = currentWeight - profile.GoalWeight;

                Console.WriteLine($"Current Weight: {currentWeight} lbs");

                if (weightChange < 0)
                {
                    Console.WriteLine($"Weight Change: Lost {Math.Abs(weightChange)} lbs");
                }
                else if (weightChange > 0)
                {
                    Console.WriteLine($"Weight Change: Gained {weightChange} lbs");
                }
                else
                {
                    Console.WriteLine("Weight Change: No change yet");
                }

                if (poundsToGoal > 0)
                {
                    Console.WriteLine($"Remaining to Goal: {poundsToGoal} lbs");
                }
                else
                {
                    Console.WriteLine($"Goal Status: Goal met or exceeded by {Math.Abs(poundsToGoal)} lbs");
                }

                Console.WriteLine($"Current BMI: {bmi:F1}");
                Console.WriteLine($"BMI Category: {GetBmiCategory(bmi)}");

                if (latestBodyFat > 0)
                {
                    Console.WriteLine($"Latest Body Fat Percentage: {latestBodyFat}%");
                }
                else
                {
                    Console.WriteLine("Latest Body Fat Percentage: Not recorded");
                }
            }
            else
            {
                Console.WriteLine("No weight progress has been recorded yet.");
            }

            Console.WriteLine();
            Console.WriteLine("--- Workout Summary ---");
            Console.WriteLine($"Workout Entries: {workoutCount}");
            Console.WriteLine($"Total Workout Minutes: {totalWorkoutMinutes}");
            Console.WriteLine($"Light Workouts: {lightWorkouts}");
            Console.WriteLine($"Moderate Workouts: {moderateWorkouts}");
            Console.WriteLine($"Hard Workouts: {hardWorkouts}");

            Console.WriteLine();
            Console.WriteLine("--- Overall Wellness Summary ---");

            if (progressCount == 0 && workoutCount == 0)
            {
                Console.WriteLine("No wellness activity has been recorded for this profile yet.");
                Console.WriteLine("Add a progress entry and a workout entry to begin tracking.");
            }
            else if (progressCount > 0 && workoutCount == 0)
            {
                Console.WriteLine("Body progress is being tracked, but no workouts have been recorded yet.");
                Console.WriteLine("Add workout entries to connect activity with weight and wellness progress.");
            }
            else if (progressCount == 0 && workoutCount > 0)
            {
                Console.WriteLine("Workout activity is being tracked, but no body progress has been recorded yet.");
                Console.WriteLine("Add progress entries to connect workouts with weight and body changes.");
            }
            else
            {
                Console.WriteLine("This profile has both body progress and workout activity recorded.");
                Console.WriteLine("The dashboard is now showing a more complete wellness picture.");
            }
        }

        // UPDATED 06/12/2026
        // Displays all progress entries currently stored.
        public void ViewProgressEntries()
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("          PROGRESS ENTRIES");
            Console.WriteLine("======================================");

            if (progressEntries.Count == 0)
            {
                Console.WriteLine("No progress entries have been added yet.");
                return;
            }

            foreach (ProgressEntry entry in progressEntries)
            {
                Console.WriteLine("\n------------------------------");
                Console.WriteLine($"Entry ID: {entry.EntryId}");
                Console.WriteLine($"Profile ID: {entry.ProfileId}");
                Console.WriteLine($"Date: {entry.EntryDate}");
                Console.WriteLine($"Weight: {entry.Weight} lbs");

                if (entry.BodyFatPercentage.HasValue)
                {
                    Console.WriteLine($"Body Fat: {entry.BodyFatPercentage.Value}%");
                }

                Console.WriteLine($"Notes: {entry.Notes}");
            }
        }

        // NEW 06/11/2026:
        // Shows all saved profiles so the user can choose who they are working with.
        private void DisplayProfileSummary()
        {
            Console.WriteLine("\n--- Current Profiles ---");

            foreach (UserProfile profile in profiles)
            {
                Console.WriteLine($"Profile ID: {profile.ProfileId} | Name: {profile.FullName} | Goal Weight: {profile.GoalWeight} lbs");
            }
        }

        // UPDATED 06/11/2026:
        // Gets the latest weight for one selected profile.
        private decimal GetCurrentWeight(int profileId)
        {
            ProgressEntry latestEntry = null;

            foreach (ProgressEntry entry in progressEntries)
            {
                if (entry.ProfileId == profileId)
                {
                    if (latestEntry == null || entry.EntryDate > latestEntry.EntryDate)
                    {
                        latestEntry = entry;
                    }
                }
            }

            if (latestEntry == null)
            {
                return 0;
            }

            return latestEntry.Weight;
        }

        // UPDATED 06/11/2026:
        // Gets the latest body fat percentage for one selected profile.
        private decimal GetLatestBodyFat(int profileId)
        {
            decimal latestBodyFat = 0;

            DateTime latestDate = DateTime.MinValue;

            foreach (ProgressEntry entry in progressEntries)
            {
                if (entry.ProfileId == profileId &&
                    entry.BodyFatPercentage.HasValue &&
                    entry.EntryDate > latestDate)
                {
                    latestBodyFat = entry.BodyFatPercentage.Value;
                    latestDate = entry.EntryDate;
                }
            }

            return latestBodyFat;
        }

        private decimal CalculateBmi(decimal weight, decimal heightInInches)
        {
            if (heightInInches <= 0)
            {
                return 0;
            }

            return (weight / (heightInInches * heightInInches)) * 703;
        }

        private string GetBmiCategory(decimal bmi)
        {
            if (bmi < 18.5m)
            {
                return "Underweight";
            }
            else if (bmi < 25)
            {
                return "Normal weight";
            }
            else if (bmi < 30)
            {
                return "Overweight";
            }
            else
            {
                return "Obese";
            }
        }

        private string GetIntensityFromUser()
        {
            while (true)
            {
                Console.WriteLine("\nChoose workout intensity:");
                Console.WriteLine("1. Light - easy movement, low effort");
                Console.WriteLine("2. Moderate - noticeable effort, but manageable");
                Console.WriteLine("3. Hard - challenging effort");
                Console.WriteLine("0. Cancel and return to main menu");
                Console.Write("Choose option 0 through 3: ");

                string choice = Console.ReadLine();

                if (choice == "1") return "Light";
                if (choice == "2") return "Moderate";
                if (choice == "3") return "Hard";
                if (choice == "0") return "Cancel";

                Console.WriteLine("Invalid option. Please choose 0 through 3.");
            }
        }

        private string GetTextOrCancel(string prompt)
        { 
            Console.Write(prompt);

            return Console.ReadLine().Trim();
        }

        private decimal GetDecimalOrCancel(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                string input = Console.ReadLine().Trim();

                if (input == "0")
                {
                    return 0;
                }

                bool isValidDecimal = decimal.TryParse(input, out decimal value);

                if (isValidDecimal && value > 0)
                { 
                    return value;
                }

                Console.WriteLine("Invalid number, enter a positive number, or 0 to cancel.");
            }
        }

        private int GetIntOrCancel(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                string input = Console.ReadLine().Trim();

                if (input == "0")
                {
                    return 0;
                }

                bool isValidNumber = int.TryParse(input, out int value);

                if (isValidNumber && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Invalid number, enter a positive whole number, or 0 to cancel.");
            }
        }

        // UPDATED 06/11/2026:
        // Saves multiple profiles, progress entries, and workout entries.
        private void SaveDataToFiles()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string profileJson = JsonSerializer.Serialize(profiles, options);

            string progressJson = JsonSerializer.Serialize(progressEntries, options);

            string workoutJson = JsonSerializer.Serialize(workoutEntries, options);

            File.WriteAllText(profileFilePath, profileJson);

            File.WriteAllText(progressFilePath, progressJson);

            File.WriteAllText(workoutFilePath, workoutJson);
        }

        private void LoadDataFromFiles()
        {
            // UPDATED 06/11/2026:
            // Loads multiple profiles from file.
            if (File.Exists(profileFilePath))
            {
                string profileJson = File.ReadAllText(profileFilePath);

                if (!string.IsNullOrWhiteSpace(profileJson))
                {
                    profiles = JsonSerializer.Deserialize<List<UserProfile>>(profileJson);

                    if (profiles == null)
                    {
                        profiles = new List<UserProfile>();
                    }
                }
            }

            if (File.Exists(progressFilePath))
            {
                string progressJson = File.ReadAllText(progressFilePath);

                if (!string.IsNullOrWhiteSpace(progressJson))
                {
                    progressEntries = JsonSerializer.Deserialize<List<ProgressEntry>>(progressJson);

                    if (progressEntries == null)
                    {
                        progressEntries = new List<ProgressEntry>();
                    }
                }
            }

            if (File.Exists(workoutFilePath))
            {
                string workoutJson = File.ReadAllText(workoutFilePath);

                if (!string.IsNullOrWhiteSpace(workoutJson))
                {
                    workoutEntries = JsonSerializer.Deserialize<List<WorkoutEntry>>(workoutJson);

                    if (workoutEntries == null)
                    {
                        workoutEntries = new List<WorkoutEntry>();
                    }
                }
            }

            // UPDATED 06/11/2026:
            // Prevents duplicate profile IDs after loading saved profiles.
            foreach (UserProfile profile in profiles)
            {
                if (profile.ProfileId >= nextProfileId)
                {
                    nextProfileId = profile.ProfileId + 1;
                }
            }

            foreach (ProgressEntry entry in progressEntries)
            {
                if (entry.EntryId >= nextProgressId)
                {
                    nextProgressId = entry.EntryId + 1;
                }
            }

            foreach (WorkoutEntry workout in workoutEntries)
            {
                if (workout.WorkoutId >= nextWorkoutId)
                {
                    nextWorkoutId = workout.WorkoutId + 1;
                }
            }
            
        }

    }

}