using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using MilkShake;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class PuzzleManager : Singleton<PuzzleManager> {

    public static GameState gameState = GameState.Menu;
    private const string alphabet = "ABCDEFGHJKLMNOPQRSTUVWXYZ";

    private int numberOfFloor = 20;
    private int tileCount = 3 * 3;

    [Header("MaterialSO")]
    [SerializeField] private MaterialSO materialSO;

    [Header("Answer Area")]
    //[SerializeField] private Transform AnswerLayoutTransform;
    private Letter[] allLetterObjects = new Letter[15];
    private List<Letter> letters = new List<Letter>();
    private int currentAnswerCount = 0;
    private int currentWord = 0;
    private int currentWordOrder = 0;
    private int currentLetter = 0;

    [Header("End Game")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private GameObject endGameExplosion;
    [SerializeField] [Range(1, 10)] private int numberOfWords;

    

    [Header("Camera")]
    [SerializeField] private ShakePreset deleteWordShakePreset;
    [SerializeField] private ShakePreset endGameShakePreset;
    [SerializeField] private GameObject camHolder;
    private Vector3 baseCamPos, baseCamRot;
    private Camera cam;
    private Shaker camShaker;
    private ShakeInstance endGameShake;
    private float lowerTime = .5f;

    private Tile hitTile;

    private Ease easeType = Ease.InExpo;

    readonly private float xOffset = 1.01f;
    readonly private float yOffset = 1f;
    readonly private float zOffset = 1.01f;


    [SerializeField] private float towerRotationAngle = 10f;
    [SerializeField] private TextMeshProUGUI goldText;
    private int gold = 10;
    private List<List<Tile>> tilePlaces = new List<List<Tile>>();
    private List<Word> words = new List<Word>();
    private List<Word> allWords;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0;i < tileCount;++i)
            tilePlaces.Add(new List<Tile>());

        allWords = new List<Word>{
                            new Word("BOOK", "Something you aren't reading since you are playing this game"),
                            new Word("FROG", "A cute little hopping amphibian"),
                            new Word("WIFE", "An extreme hardcore version of a girlfriend"),
                            new Word("SONG", "A unit of measure of time or distance"),
                            new Word("LOSS", "The act of having something.. and then suddenly not having it anymore"),
                            new Word("TWO", "The number that (usually) comes before three."),
                            new Word("NOPE", "Nah"),
                            new Word("ADULT", "Person who has stopped growing at both ends"),
                            new Word("WATER", "Life juice"),
                            new Word("NIGHT", "Time interval that most people sleep"),
                            new Word("RICH", "People who don't look at the price while shopping"),
                            new Word("NERD", "The people you pick on in high school and wind up working for as an adult"),
                            new Word("GLASSES", "Something you wear that makes you look smarter"),
                            new Word("LEAF", "A treehugger snowflake"),
                            new Word("PAJAMA", "Comfy thing to wear on bed"),
                            new Word("CHAIR", "A form of seating used to keep oneself off of the ground"),
                            new Word("TRAIN", "The vehicle that moves fast and choo choos"),
                            new Word("VAMPIRE", "Person who sucks blood for the night"),
                            new Word("ENERGY", "What little kids get when you give them anything with high amounts of sugar"),
                            new Word("CHEF", "Person who makes food taste better"),
                            new Word("HOME", "Place where you feel best"),
                            new Word("CAT", "Living ball of fur"),
                            new Word("HEAD", "Area where your best body hair grows"),
                            new Word("CHEESE", "Best use of milk, sometimes aged"),
                            new Word("DOG", "Tamed wolf"),
                            new Word("LAWYER", "Person who defends you no matter how wrong you are"),
                            new Word("MUMMY", "Pyramid resident"),
                            new Word("WHALE", "Very very big thing on the water"),
                            new Word("SOBER", "Recovery time between fits of alcoholism"),
                            new Word("NOMAD", "Full-time traveler"),
                            new Word("MOVIE", "Moving picture"),
                            new Word("QUIET", "Sssh!"),
                            new Word("CAPE", "Curtain that only fake heroes wear"),
                            new Word("BALD", "When your face is so good looking, it is taking over the rest of your head"),
                            new Word("MORNING", "Time you have to wake up"),
                            new Word("DANCE", "Keeping rhythm using one's body"),
                            new Word("VIBE", "To chill, be at peace and let life do it's thing"),
                            new Word("EXTREME", "Far beyond what is considered normal"),
                            new Word("HYPE", "To get extremely excited over something that's not that exciting"),
                            new Word("ART", "All your anxiety and chaos formed into a canvas"),
                            new Word("NOSE", "Something Voldemort does not have"),


        };

    }

    void Start() {
        Application.targetFrameRate = 120;
        cam = Camera.main;
        camShaker = cam.GetComponent<Shaker>();
        cam.fieldOfView = Screen.height / 32;
        baseCamPos = camHolder.transform.position;
        baseCamRot = camHolder.transform.rotation.eulerAngles;

        for (int i = 0;i < allLetterObjects.Length;++i)
        {
            allLetterObjects[i] = PoolManager.instance.GetLetter();
            allLetterObjects[i].gameObject.SetActive(false);
        }
    }

    //public TextMeshProUGUI frame;
    //int frameCount = 0;
    int count = 0;
    private void Update()
    {
        //if (++frameCount % 60 == 0)
        //    frame.text = (1f / Time.unscaledDeltaTime).ToString("000");
        if (Input.GetKeyDown(KeyCode.A))
        {
            print("captured");
            ScreenCapture.CaptureScreenshot("C:\\Users\\Patates\\Desktop\\Capture" + (++count).ToString() + ".png");
        }
    }

    public void HitTile(Tile tile)
    {
        hitTile = tile;
        DimTapped();
        SendToAnswerArea();
    }

    public void SetGameState(GameState newState)
    {
        gameState = newState;

        switch (newState)
        {
            case GameState.Menu:
                break;
            case GameState.Started:
                break;
            case GameState.Ended:
                break;
        }
    }

    #region Instantiate/End Puzzle

    // Instantiate tiles to list and set their positions
    private void InstantiatePuzzle() {
        Vector3 pos;
        Tile tile;
        for (int i = 0; i < tilePlaces.Count; ++i) {
            for (int j = 0; j < numberOfFloor; ++j) {
                if (j == 0)
                    pos = new Vector3((i % 3) * xOffset, 0f, (i / 3) * zOffset);
                else {
                    pos = tilePlaces[i][j - 1].transform.position;
                    pos.y -= yOffset;
                }
                tile = PoolManager.instance.GetTile();
                tile.Init(i, pos);
                tilePlaces[i].Add(tile);
            }
        }
    }


    private void InstantiateAnswerArea()
    {
        for (int i = 0;i < letters.Count;++i)
        {
            letters[i].ResetLetter();
            letters[i].gameObject.SetActive(false);
        }
        letters.Clear();

        for (int i = 0;i < words[currentWord].word.Length;++i)
        {
            letters.Add(allLetterObjects[i]);
            letters[i].gameObject.SetActive(true);
        }
    }

    // Rotate rows according to towerRotationAngle
    private void RandomizeRotation()
    {
        Vector3 middlePoint;
        float random = 0f;

        for (int i = 1; i < numberOfFloor; ++i) {
            random += towerRotationAngle;
            middlePoint = tilePlaces[4][i].transform.position;

            for (int j = 0; j < tilePlaces.Count; ++j) {
                tilePlaces[j][i].transform.RotateAround(middlePoint, Vector3.up, random);
            }
        }
    }



    // Instantiate top of columns, if all the words are filled then fill with random letters from alphabet
    private void InstantiateTop() {
        UIManager.instance.SetHint(words[currentWord].description);

        int random;

        // If words arent finished
        if (currentWordOrder != words.Count) {

            int count = tilePlaces.Count(obj => obj[0].GetFirstChar() == default);

            for (int i = 0; i < count; ++i) {
                // Find random place
                while (!(tilePlaces[random = Random.Range(0, tilePlaces.Count)][0].GetFirstChar() == default)) ;

                // If word ends fill with random letter
                if (currentWordOrder == words.Count)
                    tilePlaces[random][0].SetText(alphabet[Random.Range(0, alphabet.Length)]);

                // If there are words set from word
                else {
                    tilePlaces[random][0].SetText(words[currentWordOrder].word[currentLetter++]);

                    if (currentLetter == words[currentWordOrder].word.Length) {
                        ++currentWordOrder;
                        currentLetter = 0;
                    }
                }
            }
        }
        // If words are finished so fill with random letters
        else {
            int count = tilePlaces.Count(obj => obj[0].GetFirstChar() == default);

            for (int i = 0; i < count; ++i) {

                while (!(tilePlaces[random = Random.Range(0, tilePlaces.Count)][0].GetFirstChar() == default)) ;

                tilePlaces[random][0].SetText(alphabet[Random.Range(0, alphabet.Length)]);
            }
        }
        for (int i = 0; i < tilePlaces.Count; ++i)
            tilePlaces[i][0].ColliderChoice(true);
    }


    private void EndGame() {
        for (int i = 0; i < letters.Count; ++i)
            letters[i].SetImageColor(materialSO.CorrectMaterial.color);
        UIManager.instance.SetEndGame("YOU WON!");

        SetGameState(GameState.Ended);
        UIManager.instance.StopTimer();

        BlowUpTower();
    }

    #endregion

    #region Camera / Load Game

    public void LoadNewGame() {
        // Set timer and gold
        UIManager.instance.ResetTimer();

        goldText.text = gold.ToString();


        if (endGameShake != null)
            endGameShake.Stop(2f, true);

        currentAnswerCount = 0;

        for (int i = 0;i < tileCount;++i)
            tilePlaces[i].Clear();

        words.Clear();
        int random;
        for (int i = 0; i < numberOfWords; ++i) {
            while (words.Contains(allWords[random = Random.Range(0, allWords.Count)])) ;
            words.Add(allWords[random]);
        }

        currentWord = 0;
        currentWordOrder = 0;
        currentLetter = 0;

        InstantiatePuzzle();
        InstantiateAnswerArea();
        InstantiateTop();
        ResetTopTileMaterials();
        RandomizeRotation();

        // If camera isnt at its origin position, tween to there
        if (camHolder.transform.position != baseCamPos) {
            camHolder.transform.DOComplete();
            camHolder.transform.DOMove(baseCamPos, 2f);
            camHolder.transform.DORotate(baseCamRot, 2f, RotateMode.FastBeyond360).OnComplete(() => UIManager.instance.StartTimer());
        }
        else
            UIManager.instance.StartTimer();
    }

    public void BlowUpTower()
    {
        StartCoroutine(BlowUpTowerCoroutine());
    }

    private IEnumerator BlowUpTowerCoroutine() {

        for (int i = 0;i < tileCount;++i)
            tilePlaces[i][0].transform.DOKill();

        //endGameAudioSource.PlayOneShot(endGameStartSFX);

        //Set descend time according to row count
        camHolder.transform.DOKill();
        Vector3 newRot = camHolder.transform.rotation.eulerAngles;
        newRot.y += towerRotationAngle * tilePlaces[0].Count;
        camHolder.transform.DOMoveY(camHolder.transform.position.y - yOffset * tilePlaces[0].Count, .35f * tilePlaces[0].Count).SetEase(Ease.InOutSine);
        camHolder.transform.DORotate(newRot, .35f * tilePlaces[0].Count, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);

        Vector3 rand = new Vector3(0f, 5f, 0f);
        for (int j = 0; j < tilePlaces[0].Count; ++j) {
            // Instantiate explosion for all rows at the middle of that row
            EndGameExplosion(tilePlaces[4][j].transform.position);

            //descendAudioSource.PlayOneShot(descendTowerSFX);
            for (int i = 0; i < tilePlaces.Count; ++i) {
                rand.x = Random.Range(2f, 5f) * Mathf.Pow(-1, Random.Range(0, 2));
                rand.z = Random.Range(2f, 5f) * Mathf.Pow(-1, Random.Range(0, 2));

                // Add random force and torque
                tilePlaces[i][j].AddForceTorque(rand);

                // Destroy after some time
                tilePlaces[i][j].Release(8f);
            }
            camShaker.Shake(endGameShakePreset);
            yield return new WaitForSeconds(.3f);
        }
        endGamePanel.SetActive(true);
    }

    private async void EndGameExplosion(Vector3 pos)
    {
        ParticleSystem endGameExplosion = PoolManager.instance.GetEndGameExplosion();
        endGameExplosion.transform.SetPositionAndRotation(pos, Quaternion.Euler(Vector3.left * 90));
        await UniTask.WaitUntil(() => endGameExplosion != null && !endGameExplosion.isPlaying);
        PoolManager.instance.ReleaseEndGameExplosion(endGameExplosion);
    }


    private void LowerCamera() {
        Vector3 newRot = camHolder.transform.rotation.eulerAngles;
        newRot.y += towerRotationAngle;
        camHolder.transform.DOMoveY(camHolder.transform.position.y - yOffset, lowerTime).SetEase(easeType);
        camHolder.transform.DORotate(newRot, lowerTime).SetEase(easeType);
    }


    #endregion

    #region Delete / Switch Words


    private void DeleteWord() {
        SoundManager.instance.PlayOneShot(Source.Descend, Sound.DescendTower);

        //Handheld.Vibrate();
        for (int i = 0; i < tilePlaces.Count; ++i) {
            if (!letters.Any(obj => obj.tile.currentColumn == i))
                LowerLetter(i);
        }
        Vector3 rand = new Vector3(0f, 5f, 0f);
        for (int i = 0; i < letters.Count; ++i) {
            rand.x = Random.Range(2f, 5f) * Mathf.Pow(-1, Random.Range(0, 2));
            rand.z = Random.Range(2f, 5f) * Mathf.Pow(-1, Random.Range(0, 2));

            letters[i].tile.AddForceTorque(rand);
            letters[i].tile.Release(6f);

            tilePlaces[letters[i].tile.currentColumn].RemoveAt(0);
        }
        camShaker.Shake(deleteWordShakePreset);
        UIManager.instance.ChangeTime(TimeChange.CorrectAnswer);
        LowerCamera();
    }

    #endregion

    #region Answer Area / Tile Material
    private void SendToAnswerArea() {
        // Skip correct letters
        while (letters[currentAnswerCount].GetImageColor().Equals(materialSO.CorrectMaterial.color))
            ++currentAnswerCount;

        // Fill text and set tile
        letters[currentAnswerCount].SetText(hitTile.GetFirstChar().ToString());
        letters[currentAnswerCount++].tile = hitTile;


        if (!letters.Any(obj => obj.tile == null))
            CheckAnswer();
    }

    private void CheckAnswer(int goldIncrement = 5) {
        System.Text.StringBuilder answer = new System.Text.StringBuilder();
        for (int i = 0; i < letters.Count; ++i)
            answer.Append(letters[i].tile.GetFirstChar());

        float timeChange;
        if (answer.Equals(words[currentWord].word)) {
            // Kill Tweens
            for (int i = 0; i < tilePlaces.Count; ++i)
                tilePlaces[i][0].transform.DOKill();
            camHolder.transform.DOKill();

            gold += goldIncrement;
            goldText.text = gold.ToString();
            currentAnswerCount = 0;
            if (++currentWord == words.Count) {
                EndGame();
                return;
            }
            else {
                DeleteWord();
                InstantiateAnswerArea();
                ResetTopTileMaterials();
                InstantiateTop();
                timeChange = 5;
            }
        }
        else {
            timeChange = ChangeMaterials();
            ChangeAnswerAreaColors();
        }
        UIManager.instance.ChangeTimeAnimation(timeChange);
        UIManager.instance.EmphasizeTime();
        ResetAnswerArea();
    }

    private void ResetTopTileMaterials() {
        for (int i = 0; i < tilePlaces.Count; ++i)
            if (tilePlaces[i].Count != 0)
                tilePlaces[i][0].SetMaterial(materialSO.DefaultMaterial, materialSO.DimmedDefaultMaterial);
    }

    // Change answer are colors according to its tiles
    private void ChangeAnswerAreaColors() {
        for (int i = 0; i < letters.Count; ++i)
            letters[i].SetImageColor(letters[i].tile.GetColor());
    }


    private float ChangeMaterials() {
        // This is used for giving order wrong material to tiles as required. If there are more than required, change them to total wrong material
        int repetitiveWrongOrderTiles;
        float timeDecrement = 0;

        // First set only correct materials because we need to know how many are there before changing order wrong tiles. Thats why we use repetitiveWrongOrderTiles
        for (int i = 0; i < letters.Count; ++i) {
            if (letters[i].GetImageColor().Equals(materialSO.CorrectMaterial.color))
                continue;
            if (letters[i].tile.GetFirstChar().Equals(words[currentWord].word[i]))
                letters[i].tile.SetMaterial(materialSO.CorrectMaterial);
        }

        // Set totalWrong and orderWrong
        for (int i = 0; i < letters.Count; ++i)
        {
            if (letters[i].tile.GetMaterial().Equals(materialSO.CorrectMaterial))
                continue;

            repetitiveWrongOrderTiles = tilePlaces.Count(obj => !obj[0].Equals(letters[i].tile) && obj[0].GetFirstChar().Equals(letters[i].tile.GetFirstChar()) && (obj[0].GetMaterial().Equals(materialSO.OrderWrongMaterial) || obj[0].GetMaterial().Equals(materialSO.DimmedOrderWrongMaterial)));
            if (words[currentWord].word.Contains(letters[i].tile.GetFirstChar()) && letters.Count(obj => obj.tile.GetFirstChar().Equals(letters[i].tile.GetFirstChar()) && obj.tile.GetMaterial().Equals(materialSO.CorrectMaterial)) + repetitiveWrongOrderTiles < words[currentWord].word.Count(obj => obj == letters[i].tile.GetFirstChar()))
            {
                letters[i].tile.SetMaterial(materialSO.OrderWrongMaterial, materialSO.DimmedOrderWrongMaterial);
                timeDecrement -= UIManager.instance.ChangeTime(TimeChange.OrderWrong);
            }
            else
            {
                letters[i].tile.SetMaterial(materialSO.TotalWrongMaterial);
                timeDecrement -= UIManager.instance.ChangeTime(TimeChange.TotalWrong);
            }
        }
        return timeDecrement;
    }

    // Lower letter at the top of the column
    private void LowerLetter(int column)
    {
        // Move/rotate to position/rotation of 1th index
        tilePlaces[column][0].transform.DOMove(tilePlaces[column][1].transform.position, lowerTime).SetEase(easeType).OnComplete(() =>
        {
            ParticleSystem tileSmoke = PoolManager.instance.GetTileSmoke();
            tileSmoke.transform.SetPositionAndRotation(tilePlaces[column][1].transform.position, Quaternion.Euler(Vector3.left * 90));
        });

        tilePlaces[column][0].transform.DORotate(tilePlaces[column][1].transform.rotation.eulerAngles, lowerTime).SetEase(easeType);
        
        // Remove from list and destroy
        Tile tile = tilePlaces[column][1];
        tilePlaces[column].RemoveAt(1);
        tile.Release(lowerTime);
    }

    public void ResetAnswerArea()
    {
        if (gameState == GameState.Ended)
            return;

        Material letterMaterial;
        for (int i = 0; i < letters.Count; ++i) {
            if (letters[i].tile == null)
                continue;

            letterMaterial = letters[i].tile.GetMaterial();
            // If letter is correct then skip it
            if (letterMaterial.Equals(materialSO.CorrectMaterial))
                continue;
            // If it is order wrong letter then bright its tile
            if (!letterMaterial.Equals(materialSO.TotalWrongMaterial))
                BrightenTapped(letters[i].tile);

            // Reset answer area text/image
            letters[i].ResetImageColor();
            letters[i].tile = null;
            letters[i].SetText();
        }
        currentAnswerCount = 0;
    }


    #endregion

    #region Skip / Hint

    public void SkipButton() {
        if (gameState == GameState.Ended || gold < 10)
            return;
        gold -= 10;
        goldText.text = gold.ToString();

        UIManager.instance.CloseButtonForSeconds(InGameButton.Skip, lowerTime);

        for (int i = 0; i < words[currentWord].word.Length; ++i) {
            if (letters[i].tile == null)
                continue;
            letters[i].tile.SetMaterial(materialSO.DefaultMaterial, materialSO.DimmedDefaultMaterial);
            letters[i].tile = null;
        }
        for (int i = 0; i < words[currentWord].word.Length; ++i) {
            for (int j = 0; j < tilePlaces.Count; ++j) {
                if (tilePlaces[j][0].GetFirstChar().Equals(words[currentWord].word[i]) && !tilePlaces[j][0].GetMaterial().Equals(materialSO.CorrectMaterial)) {
                    letters[i].tile = tilePlaces[j][0];
                    letters[i].tile.SetMaterial(materialSO.CorrectMaterial);
                    letters[i].SetText(letters[i].tile.GetFirstChar().ToString());
                    break;
                }
            }
        }
        CheckAnswer(0);
    }

    public void HintButton() {
        if (gameState == GameState.Ended || gold < 5)
            return;
        gold -= 5;
        goldText.text = gold.ToString();
        int random;
        while (letters[random = Random.Range(0, letters.Count)].GetImageColor().Equals(materialSO.CorrectMaterial.color)) ;
        for (int i = 0; i < tilePlaces.Count; ++i) {
            if (tilePlaces[i][0].GetFirstChar().Equals(words[currentWord].word[random]) && !tilePlaces[i][0].GetMaterial().Equals(materialSO.CorrectMaterial)) {
                if (letters[random].tile != null)
                    letters[random].tile.Brighten();
                tilePlaces[i][0].ColliderChoice(false);
                letters[random].tile = tilePlaces[i][0];
                letters[random].SetImageColor(materialSO.CorrectMaterial.color);
                letters[random].tile.SetMaterial(materialSO.CorrectMaterial);
                letters[random].SetText(letters[random].tile.GetFirstChar().ToString());
                break;
            }
        }
        if (letters.Count(obj => obj.tile != null) == words[currentWord].word.Length)
            CheckAnswer();
    }

    #endregion

    #region Tower Color Changes

    private void DimTapped(bool colliderChoice = false) {
        hitTile.ColliderChoice(colliderChoice);
        hitTile.Dim();
    }

    private void BrightenTapped(Tile Letter, bool colliderChoice = true) {
        Letter.Brighten();
        Letter.ColliderChoice(colliderChoice);
    }


    #endregion
}
