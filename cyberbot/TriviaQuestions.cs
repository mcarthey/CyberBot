public static class TriviaQuestions
{
    private static readonly Dictionary<string, string> QuestionsAndAnswers = new Dictionary<string, string>()
    {
        { "What was the name of the famous female pirate who terrorized the Caribbean in the early 18th century?", "Anne Bonny" },
        { "What pirate term refers to a small, fast ship used by pirates for raids and reconnaissance?", "Sloop" },
        { "Which famous pirate captain was known for his association with the 'Bloody Red Banner' flag?", "Bartholomew Roberts" },
        { "What is the traditional pirate greeting or expression of affirmation?", "Arrr!" },
        { "What was the name of the pirate who, according to legend, buried treasure on Oak Island?", "Captain Kidd" },
        { "What is the term for the pirate practice of leaving a crew member marooned on a deserted island?", "Marooning" },
        { "What pirate term refers to a group of ships acting together, especially in a raid?", "Flotilla" },
        { "Which pirate was known for his large, bushy beard, which he often set on fire to intimidate his enemies?", "Blackbeard" },
        { "What is the name of the pirate code that was used by many pirate crews as a set of rules and guidelines?", "The Pirate's Code" },
        { "Which famous pirate was known for his use of a glass eye, which he would remove and show to his enemies to frighten them?", "John 'Mad Jack' Rackham" },
        { "What is the term for a pirate's share of the plunder, typically one equal part?", "Loot" },
        { "Which pirate was known for his association with the pirate flag featuring a skull and crossed cutlasses?", "Edward Low" },
        { "What is the term for a pirate who operated with the approval of a government, often attacking ships of enemy nations?", "Privateer" },
        { "What is the name of the pirate who was known for his use of a flag featuring a skeleton holding a spear, symbolizing death and the afterlife?", "Edward England" },
        { "What is the term for a pirate who operates by attacking ships from a hidden or concealed position?", "Ambusher" },
        { "What was the nickname of the famous pirate William Kidd, who was hanged for piracy in 1701?", "Captain Kidd" },
        { "What is the term for a pirate who raids while disguised as a regular merchant ship to surprise their enemies?", "False Flag" },
        { "Which pirate was known for his association with the pirate flag featuring a skull and crossed bones, known as the 'Jolly Roger'?", "Thomas Tew" },
        { "What is the name of the pirate who was known for his use of a flag featuring a skeleton holding an hourglass, symbolizing the limited time to live?", "Walter Kennedy" },
        { "What is the term for a pirate who engages in piracy for personal gain rather than as part of a crew or fleet?", "Rogue Pirate" },
        { "What is the name of the pirate who was known for his use of a flag featuring a skull and crossed bones, known as the 'Jolly Roger'?", "Nathaniel North" },
        { "Which famous female pirate was known for disguising herself as a man and participating in pirate raids in the early 19th century?", "Mary Read" },
        { "What is the term for a pirate who operates by attacking ships from a position of authority, such as a government or military official?", "Corrupt Official" },
        { "Which pirate, known for his association with the pirate flag featuring a skull and crossed swords, was nicknamed 'The King of Pirates'?", "Henry Every" },
        { "This pirate, often referred to as 'Black Sam', was known for his association with the pirate flag featuring a skull and crossed swords.", "Samuel Bellamy" },
        { "Who was known as 'The Sea Rover' and used a flag featuring a skull, crossed bones, and an hourglass, symbolizing the inevitability of death?", "Richard Worley" },
        { "Which pirate, nicknamed 'The Gentleman Pirate', was associated with the pirate flag featuring a skull, crossed swords, and an hourglass?", "Stede Bonnet" },
        { "This pirate, known as 'The Great Pirate Roberts', was associated with the pirate flag featuring a skull, crossed bones, and an hourglass. Who was he?", "Christopher Moody" },
        { "Known as 'The Gentleman of Fortune' and associated with the pirate flag featuring a skull, crossed swords, and an hourglass. Who was he?", "Charles Vane" },
    };

    public static string GetRandomQuestion(Random random)
    {
        var questionIndex = random.Next(0, QuestionsAndAnswers.Count);
        return QuestionsAndAnswers.Keys.ElementAt(questionIndex);
    }

    public static string GetAnswer(string question)
    {
        return QuestionsAndAnswers[question];
    }
}
