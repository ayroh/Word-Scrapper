

public class Word
{
    public string word, description;
    public Word(string tempWord, string tempDescription)
    {
        word = tempWord;
        description = tempDescription;
    }
}

public enum TimeChange
{
    OrderWrong,
    TotalWrong,
    CorrectAnswer
}

public enum GameState
{
    Menu,
    Started,
    Ended
}

public enum Source
{
    EndGame,
    Descend
}

public enum Sound
{
    DescendTower,
    EndGameStart,
    EndGameDynamite
}

public enum InGameButton
{
    Skip,
    Hint
}