using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;
using MilkShake;
using UnityEngine.EventSystems;

public class PuzzleManagerScript : MonoBehaviour {
    public class Word {
        public string word, description;
        public Word(string tempWord, string tempDescription) {
            word = tempWord;
            description = tempDescription;
        }
    }


    [Header("Tile")]
    [SerializeField] private GameObject Tile;
    [SerializeField] private Transform TileParent;
    [SerializeField] private GameObject TileSmoke;
    private int numberOfFloor = 20;
    private int tileCount = 3 * 3;

    [Header("Materials")]
    [SerializeField] private Material DefaultMaterial;
    [SerializeField] private Material DimmedDefaultMaterial;
    [SerializeField] private Material TotalWrongMaterial;
    [SerializeField] private Material CorrectMaterial;
    [SerializeField] private Material OrderWrongMaterial;
    [SerializeField] private Material DimmedOrderWrongMaterial;

    [Header("Answer Area")]
    [SerializeField] private Transform AnswerLayoutTransform;
    [SerializeField] private GameObject AnswerAreaTile;
    private AnswerAreaTileScript[] AnswerAreaObjects;
    private int currentAnswerCount = 0;
    private int currentWord = 0;
    private int currentWordOrder = 0;
    private int currentLetter = 0;

    [Header("End Game")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private GameObject endGameExplosion;
    [SerializeField] private TextMeshProUGUI endGameText;
    [SerializeField] [Range(1, 10)] private int numberOfWords;
    private bool gameEnded = false;

    [Header("Hint")]
    [SerializeField] private TextMeshProUGUI HintText;
    [SerializeField] private TextMeshProUGUI Hint;

    [Header("Camera")]
    [SerializeField] private ShakePreset deleteWordShakePreset;
    [SerializeField] private ShakePreset endGameShakePreset;
    [SerializeField] private GameObject camHolder;
    private Vector3 baseCamPos, baseCamRot;
    private Camera cam;
    private Shaker camShaker;
    private ShakeInstance endGameShake;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float maxTimer = 30f;
    [SerializeField] private float correctAnswerTimerIncrease = 5f;
    [SerializeField] private float orderWrongAnswerTimerDecrease = .5f;
    [SerializeField] private float totalWrongAnswerTimerDecrease = 1f;
    [SerializeField] private GameObject timeAnimationPrefab;
    IEnumerator timerCoroutine = null;
    private float lowerTime = .5f;
    private float timer;

    [Header("Buttons")]
    [SerializeField] private UnityEngine.UI.Button skipButton;
    [SerializeField] private UnityEngine.UI.Button hintButton;

    [Header("SFX")]
    [SerializeField] private AudioClip descendTowerSFX;
    [SerializeField] private AudioClip endGameStartSFX;
    [SerializeField] private AudioClip endGameDynamiteSFX;
    [SerializeField] private AudioSource descendAudioSource;
    [SerializeField] private AudioSource endGameAudioSource;

    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI vibrationText;
    [SerializeField] private TextMeshProUGUI soundText;
    private bool vibrationChoice = true;



    private RaycastHit[] hit = null;
    private Ease easeType = Ease.InExpo;

    readonly private float xOffset = 1.01f;
    readonly private float yOffset = 1f;
    readonly private float zOffset = 1.01f;


    [SerializeField] private float towerRotationAngle = 10f;
    [SerializeField] private TextMeshProUGUI goldText;
    private int gold = 10;
    private List<List<TileScript>> tilePlaces;
    private List<Word> words, allWords;

    void Start() {
        Application.targetFrameRate = 120;
        cam = Camera.main;
        camShaker = cam.GetComponent<Shaker>();
        cam.fieldOfView = Screen.height / 32;
        baseCamPos = camHolder.transform.position;
        baseCamRot = camHolder.transform.rotation.eulerAngles;
        hit = new RaycastHit[1];
        //LoadNewGame();
    }


    private void Update() {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(0) && !EventSystem.current.IsPointerOverGameObject(-1) && Physics.RaycastNonAlloc(cam.ScreenPointToRay(Input.mousePosition), hit) == 1) {
            if (AnswerAreaObjects.All(obj => obj.tile != null))
                return;
            DimTapped();
            SendToAnswerArea();
        }
    }

    #region Instantiate/End Puzzle

    // Instantiate tiles to list and set their positions
    private void InstantiatePuzzle() {
        Vector3 pos;
        TileScript obj;
        for (int i = 0; i < tilePlaces.Count; ++i) {
            for (int j = 0; j < numberOfFloor; ++j) {
                if (j == 0)
                    pos = new Vector3((i % 3) * xOffset, 0f, (i / 3) * zOffset);
                else {
                    pos = tilePlaces[i][j - 1].transform.position;
                    pos.y -= yOffset;
                }
                obj = Instantiate(Tile, pos, Quaternion.identity, TileParent).GetComponent<TileScript>();
                obj.Init(i);
                tilePlaces[i].Add(obj);
            }
        }
    }


    private void InstantiateAnswerArea() {
        for (int i = AnswerLayoutTransform.childCount - 1; i >= 0; --i)
            Destroy(AnswerLayoutTransform.GetChild(i).gameObject);
        AnswerAreaObjects = new AnswerAreaTileScript[words[currentWord].word.Length];
        for (int i = 0; i < words[currentWord].word.Length; ++i)
            AnswerAreaObjects[i] = Instantiate(AnswerAreaTile, AnswerLayoutTransform).GetComponent<AnswerAreaTileScript>();
    }

    // Rotate rows according to towerRotationAngle
    private void RandomizeRotation() {
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


    private readonly string alphabet = "ABCDEFGHJKLMNOPQRSTUVWXYZ";

    // Instantiate top of columns, if all words are filled then fill with random letters from alphabet
    private void InstantiateTop() {
        Hint.text = words[currentWord].description;
        int random;
        // If words arent finished
        if (currentWordOrder != words.Count) {
            int count = tilePlaces.Count(obj => obj[0].GetText() == default);
            for (int i = 0; i < count; ++i) {
                // Find random place
                while (!(tilePlaces[random = Random.Range(0, tilePlaces.Count)][0].GetText() == default)) ;

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
            int count = tilePlaces.Count(obj => obj[0].GetText() == default);
            for (int i = 0; i < count; ++i) {
                while (!(tilePlaces[random = Random.Range(0, tilePlaces.Count)][0].GetText() == default)) ;
                tilePlaces[random][0].SetText(alphabet[Random.Range(0, alphabet.Length)]);
            }
        }
        for (int i = 0; i < tilePlaces.Count; ++i)
            tilePlaces[i][0].ColliderChoice(true);
    }



    private void DeleteAll() {
        if (tilePlaces != null) {
            for (int i = 0; i < tilePlaces.Count; ++i) {
                for (int j = 0; j < tilePlaces[i].Count; ++j) {
                    Destroy(tilePlaces[i][j]);
                }
            }
        }
    }

    private void EndGame() {
        for (int i = 0; i < AnswerAreaObjects.Length; ++i)
            AnswerAreaObjects[i].SetImageColor(CorrectMaterial.color);
        endGameText.text = "YOU WON!";
        StartCoroutine(BlowUpTower());
    }

    #endregion

    #region Camera / Load Game

    public void LoadNewGame() {
        // Set timer and gold
        timer = maxTimer;
        goldText.text = gold.ToString();

        // Delete all tiles
        DeleteAll();

        if (endGameShake != null)
            endGameShake.Stop(2f, true);

        // If camera isnt at its origin position, tween to there
        if (camHolder.transform.position != baseCamPos) {
            camHolder.transform.DOComplete();
            camHolder.transform.DOMove(baseCamPos, 2f);
            camHolder.transform.DORotate(baseCamRot, 2f, RotateMode.FastBeyond360).OnComplete(() => gameEnded = false);
        }
        currentAnswerCount = 0;
        tilePlaces = new List<List<TileScript>>();
        for (int i = 0; i < tileCount; ++i)
            tilePlaces.Add(new List<TileScript>());
        allWords = new List<Word>{
                            new Word("BOOK", "Something you aren't reading since you are playing this game"),
                            new Word("FROG", "A cute little hopping amphibian"),
                            new Word("USER", "A person who is only nice when he needs something"),
                            new Word("WIFE", "An extreme hardcore version of a girlfriend"),
                            new Word("MALL", "A place where teenagers go to waste their lives away"),
                            new Word("SONG", "A unit of measure of time or distance"),
                            new Word("LOSS", "The act of having something.. and then suddenly not having it anymore"),
                            new Word("TWO", "The number that (usually) comes before three."),
                            new Word("SIR", "The proper way for a submissive to greet a Dominant"),
                            new Word("LADY", "An elegant woman who uses her femininity in the most endearing way possible"),
                            //new Word("ELA", "Worlds most beautiful girl"),
                            //new Word("PHONE", "Worlds most popular electronic device"),
                            //new Word("INSTALL", "process after you download"),
                            //new Word("HOTSPOT", "so many people use wi-fi ......."),

        };
        words = new List<Word>();
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
        RandomizeRotation();
        StartCoroutine(timerCoroutine = Timer());
    }



    private IEnumerator BlowUpTower() {
        gameEnded = true;
        if (timerCoroutine != null) {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
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
            Instantiate(endGameExplosion, tilePlaces[4][j].transform.position, Quaternion.Euler(Vector3.left * 90));

            //descendAudioSource.PlayOneShot(descendTowerSFX);
            for (int i = 0; i < tilePlaces.Count; ++i) {
                rand.x = Random.Range(2f, 5f) * Mathf.Pow(-1, Random.Range(0, 2));
                rand.z = Random.Range(2f, 5f) * Mathf.Pow(-1, Random.Range(0, 2));

                // Add random force and torque
                tilePlaces[i][j].gameObject.AddComponent<Rigidbody>().AddForce(rand, ForceMode.VelocityChange);
                tilePlaces[i][j].gameObject.GetComponent<Rigidbody>().AddTorque(rand, ForceMode.VelocityChange);

                // Destroy after some time
                Destroy(tilePlaces[i][j].gameObject, 8f);
            }
            camShaker.Shake(endGameShakePreset);
            yield return new WaitForSeconds(.3f);
        }
        endGamePanel.SetActive(true);
    }


    private void LowerCamera() {
        Vector3 newRot = camHolder.transform.rotation.eulerAngles;
        newRot.y += towerRotationAngle;
        camHolder.transform.DOMoveY(camHolder.transform.position.y - yOffset, lowerTime).SetEase(easeType);
        camHolder.transform.DORotate(newRot, lowerTime).SetEase(easeType);
    }


    #endregion

    #region Timer

    private Tween tw;
    [SerializeField] private Canvas canvas;
    private float timeAnimationTime = 1f;
    private void EmphasizeTime() {
        if(tw == null || !tw.IsPlaying())
            (tw = DOTween.To(() => timerText.fontSize, x => timerText.fontSize = x, 110, .15f)).SetAutoKill(false).OnComplete(() => tw.PlayBackwards());
    }

    private IEnumerator Timer() {
        while (0f < timer) {
            timerText.text = ((int)timer + 1).ToString();
            timer -= Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < tileCount; ++i)
            tilePlaces[i][0].transform.DOKill();

        endGameText.text = "YOU LOST!";
        timerCoroutine = null;
        StartCoroutine(BlowUpTower());
    }

    private void ChangeTimeAnimation(float DecrementTime) {
        TextMeshProUGUI timeAnimationTMP = Instantiate(timeAnimationPrefab, canvas.transform as RectTransform).GetComponent<TextMeshProUGUI>();
        Vector3 newPosition;
        newPosition = timerText.rectTransform.position;
        Color newColor;
        if (DecrementTime < 0) {
            timeAnimationTMP.rectTransform.position = timerText.rectTransform.position;
            newPosition.x += 150;
            timeAnimationTMP.text = DecrementTime.ToString();
            newColor = Color.red;
            newColor.a = .4f;
            timeAnimationTMP.color = newColor;
            newColor.a = .9f;
        }
        else {
            newPosition.x -= 150;
            timeAnimationTMP.rectTransform.position = newPosition;
            newPosition.x += 150;
            timeAnimationTMP.text = "+" + DecrementTime.ToString();
            newColor = Color.green;
            timeAnimationTMP.color = newColor;
            newColor.a = .1f;
        }
        DOTween.To(() => timeAnimationTMP.rectTransform.position, x => timeAnimationTMP.rectTransform.position = x, newPosition, timeAnimationTime);
        DOTween.To(() => timeAnimationTMP.color, x => timeAnimationTMP.color = x, newColor, timeAnimationTime).OnComplete(() => Destroy(timeAnimationTMP.gameObject));
    }

    #endregion

    #region Delete / Switch Words


    private void DeleteWord() {
        descendAudioSource.PlayOneShot(descendTowerSFX);
        Handheld.Vibrate();
        for (int i = 0; i < tilePlaces.Count; ++i) {
            if (!AnswerAreaObjects.Any(obj => obj.tile.currentColumn == i))
                LowerLetter(i);
        }
        Vector3 rand = new Vector3(0f, 5f, 0f);
        for (int i = 0; i < AnswerAreaObjects.Length; ++i) {
            rand.x = Random.Range(2f, 5f) * Mathf.Pow(-1, Random.Range(0, 2));
            rand.z = Random.Range(2f, 5f) * Mathf.Pow(-1, Random.Range(0, 2));
            AnswerAreaObjects[i].tile.gameObject.AddComponent<Rigidbody>().AddForce(rand, ForceMode.VelocityChange);
            AnswerAreaObjects[i].tile.gameObject.GetComponent<Rigidbody>().AddTorque(rand, ForceMode.VelocityChange);
            Destroy(AnswerAreaObjects[i].tile.gameObject, 6f);

            tilePlaces[AnswerAreaObjects[i].tile.currentColumn].RemoveAt(0);
        }
        camShaker.Shake(deleteWordShakePreset);
        timer += correctAnswerTimerIncrease;
        LowerCamera();
    }

    #endregion

    #region Answer Area / Tile Material
    private void SendToAnswerArea() {
        // Skip correct letters
        while (AnswerAreaObjects[currentAnswerCount].GetImageColor().Equals(CorrectMaterial.color))
            ++currentAnswerCount;

        // Fill text and set tile
        AnswerAreaObjects[currentAnswerCount].SetText(hit[0].transform.GetComponentInChildren<TextMeshPro>().text[0].ToString());
        AnswerAreaObjects[currentAnswerCount++].tile = hit[0].transform.GetComponent<TileScript>();


        if (!AnswerAreaObjects.Any(obj => obj.tile == null))
            CheckAnswer();
    }

    private void CheckAnswer(int goldIncrement = 5) {
        System.Text.StringBuilder answer = new System.Text.StringBuilder();
        for (int i = 0; i < AnswerAreaObjects.Length; ++i)
            answer.Append(AnswerAreaObjects[i].tile.GetText());

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
        ChangeTimeAnimation(timeChange);
        EmphasizeTime();
        ResetAnswerArea();
    }

    private void ResetTopTileMaterials() {
        for (int i = 0; i < tilePlaces.Count; ++i)
            if (tilePlaces[i].Count != 0)
                tilePlaces[i][0].SetMaterial(DefaultMaterial, DimmedDefaultMaterial);
    }

    // Change answer are colors according to its tiles
    private void ChangeAnswerAreaColors() {
        for (int i = 0; i < AnswerAreaObjects.Length; ++i)
            AnswerAreaObjects[i].SetImageColor(AnswerAreaObjects[i].tile.GetColor());
    }


    private float ChangeMaterials() {
        // This is used for giving order wrong material to tiles as required. If there are more than required, change them to total wrong material
        int repetitiveWrongOrderTiles;
        float timeDecrement = 0;
        // First set only correct materials because we need to know how many are there before changing order wrong tiles. Thats why we use repetitiveWrongOrderTiles
        for (int i = 0; i < AnswerAreaObjects.Length; ++i) {
            if (AnswerAreaObjects[i].GetImageColor().Equals(CorrectMaterial.color))
                continue;
            if (AnswerAreaObjects[i].tile.GetText().Equals(words[currentWord].word[i]))
                AnswerAreaObjects[i].tile.SetMaterial(CorrectMaterial);
        }

        // Set totalWrong and orderWrong
        for (int i = 0; i < AnswerAreaObjects.Length; ++i) {
            if (AnswerAreaObjects[i].tile.GetMaterial().Equals(CorrectMaterial))
                continue;

            repetitiveWrongOrderTiles = tilePlaces.Count(obj => !obj[0].Equals(AnswerAreaObjects[i].tile) && obj[0].GetText().Equals(AnswerAreaObjects[i].tile.GetText()) && (obj[0].GetMaterial().Equals(OrderWrongMaterial) || obj[0].GetMaterial().Equals(DimmedOrderWrongMaterial)));
            if (words[currentWord].word.Contains(AnswerAreaObjects[i].tile.GetText()) && AnswerAreaObjects.Count(obj => obj.tile.GetText().Equals(AnswerAreaObjects[i].tile.GetText()) && obj.tile.GetMaterial().Equals(CorrectMaterial)) + repetitiveWrongOrderTiles < words[currentWord].word.Count(obj => obj == AnswerAreaObjects[i].tile.GetText())) {
                AnswerAreaObjects[i].tile.SetMaterial(OrderWrongMaterial, DimmedOrderWrongMaterial);
                timer -= orderWrongAnswerTimerDecrease;
                timeDecrement -= orderWrongAnswerTimerDecrease;
            }
            else {
                AnswerAreaObjects[i].tile.SetMaterial(TotalWrongMaterial);
                timer -= totalWrongAnswerTimerDecrease;
                timeDecrement -= totalWrongAnswerTimerDecrease;
            }
        }
        return timeDecrement;
    }

    // Lower letter at the top of the column
    private void LowerLetter(int column) {
        // Move/rotate to position/rotation of 1th index
        tilePlaces[column][0].transform.DOMove(tilePlaces[column][1].transform.position, lowerTime).SetEase(easeType).OnComplete(() => Instantiate(TileSmoke, tilePlaces[column][1].transform.position, Quaternion.Euler(Vector3.left * 90)));
        tilePlaces[column][0].transform.DORotate(tilePlaces[column][1].transform.rotation.eulerAngles, lowerTime).SetEase(easeType);
        
        // Remove from list and destroy
        GameObject obj = tilePlaces[column][1].gameObject;
        tilePlaces[column].RemoveAt(1);
        Destroy(obj, lowerTime);
    }

    public void ResetAnswerArea() {
        if (gameEnded)
            return;

        Material answerAreaTileMaterial;
        for (int i = 0; i < AnswerAreaObjects.Length; ++i) {
            if (AnswerAreaObjects[i].tile == null)
                continue;

            answerAreaTileMaterial = AnswerAreaObjects[i].tile.GetMaterial();
            // If letter is correct then skip it
            if (answerAreaTileMaterial.Equals(CorrectMaterial))
                continue;
            // If it is order wrong letter then bright its tile
            if (!answerAreaTileMaterial.Equals(TotalWrongMaterial))
                BrightenTapped(AnswerAreaObjects[i].tile);

            // Reset answer area text/image
            AnswerAreaObjects[i].SetImageColor();
            AnswerAreaObjects[i].tile = null;
            AnswerAreaObjects[i].SetText();
        }
        currentAnswerCount = 0;
    }


    #endregion

    #region Skip / Hint

    public void SkipButton() {
        if (gameEnded || gold < 10)
            return;
        gold -= 10;
        goldText.text = gold.ToString();
        StartCoroutine(CloseButtonsCoroutine());
        for (int i = 0; i < words[currentWord].word.Length; ++i) {
            if (AnswerAreaObjects[i].tile == null)
                continue;
            AnswerAreaObjects[i].tile.SetMaterial(DefaultMaterial, DimmedDefaultMaterial);
            AnswerAreaObjects[i].tile = null;
        }
        for (int i = 0; i < words[currentWord].word.Length; ++i) {
            for (int j = 0; j < tilePlaces.Count; ++j) {
                if (tilePlaces[j][0].GetText().Equals(words[currentWord].word[i]) && !tilePlaces[j][0].GetMaterial().Equals(CorrectMaterial)) {
                    AnswerAreaObjects[i].tile = tilePlaces[j][0];
                    AnswerAreaObjects[i].tile.SetMaterial(CorrectMaterial);
                    AnswerAreaObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = AnswerAreaObjects[i].tile.GetText().ToString();
                    break;
                }
            }
        }
        CheckAnswer(0);
    }

    private IEnumerator CloseButtonsCoroutine() {
        skipButton.enabled = false;
        yield return new WaitForSeconds(lowerTime);
        skipButton.enabled = true;
    }

    public void HintButton() {
        if (gameEnded || gold < 5)
            return;
        gold -= 5;
        goldText.text = gold.ToString();
        int random;
        while (AnswerAreaObjects[random = Random.Range(0, AnswerAreaObjects.Length)].GetImageColor().Equals(CorrectMaterial.color)) ;
        for (int i = 0; i < tilePlaces.Count; ++i) {
            if (tilePlaces[i][0].GetText().Equals(words[currentWord].word[random]) && !tilePlaces[i][0].GetMaterial().Equals(CorrectMaterial)) {
                if (AnswerAreaObjects[random].tile != null)
                    AnswerAreaObjects[random].tile.Brighten();
                tilePlaces[i][0].ColliderChoice(false);
                AnswerAreaObjects[random].tile = tilePlaces[i][0];
                AnswerAreaObjects[random].SetImageColor(CorrectMaterial.color);
                AnswerAreaObjects[random].tile.SetMaterial(CorrectMaterial);
                AnswerAreaObjects[random].GetComponentInChildren<TextMeshProUGUI>().text = AnswerAreaObjects[random].tile.GetText().ToString();
                break;
            }
        }
        if (AnswerAreaObjects.Count(obj => obj.tile != null) == words[currentWord].word.Length)
            CheckAnswer();
    }

    #endregion

    #region Panel Buttons

    public void StopTime() {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    public void StartTime(){
        if (timerCoroutine != null)
            StartCoroutine(timerCoroutine);
    }

    public void VibrationToggle() {
        vibrationChoice = !vibrationChoice;
        if (vibrationChoice)
            vibrationText.text = "Vibration On";
        else
            vibrationText.text = "Vibration Off";
    }

    public void SoundToggle() {
        if(descendAudioSource.volume == 0) {
            soundText.text = "Sound On";
            descendAudioSource.volume = .5f;
            endGameAudioSource.volume = .5f;
        }
        else {
            soundText.text = "Sound Off";
            descendAudioSource.volume = 0f;
            endGameAudioSource.volume = 0f;
        }
    }

    #endregion

    #region Tower Color Changes

    private void DimTapped(bool colliderChoice = false) {
        TileScript tile = hit[0].transform.GetComponent<TileScript>();
        tile.ColliderChoice(colliderChoice);
        tile.Dim();
    }

    private void BrightenTapped(TileScript AnswerAreaTile, bool colliderChoice = true) {
        AnswerAreaTile.Brighten();
        AnswerAreaTile.ColliderChoice(colliderChoice);
    }


    #endregion
}
