using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{

    [Header("Tile")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform tileParent;

    [Header("Answer Area Tile")]
    [SerializeField] private Letter letterPrefab;
    [SerializeField] private Transform letterParent;

    [Header("Tile Smoke")]
    [SerializeField] private ParticleSystem tileSmokePrefab;
    [SerializeField] private Transform tileSmokeParent;

    [Header("Tile Animation")]
    [SerializeField] private TextMeshProUGUI timeAnimationPrefab;
    [SerializeField] private Transform timeAnimationParent;

    [Header("End Smoke")]
    [SerializeField] private ParticleSystem endGameExplosionPrefab;
    [SerializeField] private Transform endGameExplosionParent;

    // Pools
    private Queue<Tile> availableTiles = new Queue<Tile>();
    private Queue<Letter> availableLetters = new Queue<Letter>();
    private Queue<ParticleSystem> availableTileSmokes = new Queue<ParticleSystem>();
    private Queue<TextMeshProUGUI> availableTimeAnimations = new Queue<TextMeshProUGUI>();
    private Queue<ParticleSystem> availableEndGameExplosions = new Queue<ParticleSystem>();


    private void Start()
    {
        PoolCubes();
    }

    private void PoolCubes()
    {
        for(int i = 0;i < tileParent.childCount;++i)
            availableTiles.Enqueue(tileParent.GetChild(i).GetComponent<Tile>());
    }

    #region Tile
    private Tile CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, tileParent);
        return tile;
    }

    public Tile GetTile()
    {
        Tile tile;
        if (availableTiles.Count == 0)
        {
            tile = CreateTile();
        }
        else
        {
            tile = availableTiles.Dequeue();
        }
        tile.gameObject.SetActive(true);
        return tile;
    }

    public void ReleaseTile(Tile tile)
    {
        tile.gameObject.SetActive(false);
        tile.ResetTile();

        availableTiles.Enqueue(tile);
    }

    #endregion


    #region Letter
    private Letter CreateLetter()
    {
        Letter letter = Instantiate(letterPrefab, letterParent);
        return letter;
    }

    public Letter GetLetter()
    {
        Letter letter;
        if (availableLetters.Count == 0)
        {
            letter = CreateLetter();
        }
        else
        {
            letter = availableLetters.Dequeue();
        }
        letter.gameObject.SetActive(true);
        return letter;
    }

    public void ReleaseLetter(Letter letter)
    {
        letter.gameObject.SetActive(false);
        letter.ResetLetter();

        availableLetters.Enqueue(letter);
    }

    #endregion

    #region TileSmoke
    private ParticleSystem CreateTileSmoke()
    {
        ParticleSystem tileSmoke = Instantiate(tileSmokePrefab, tileSmokeParent);
        return tileSmoke;
    }

    public ParticleSystem GetTileSmoke()
    {
        ParticleSystem tileSmoke;
        if (availableTileSmokes.Count == 0)
        {
            tileSmoke = CreateTileSmoke();
        }
        else
        {
            tileSmoke = availableTileSmokes.Dequeue();
        }
        tileSmoke.gameObject.SetActive(true);
        return tileSmoke;
    }

    public void ReleaseTileSmoke(ParticleSystem tileSmoke)
    {
        tileSmoke.Stop();
        tileSmoke.gameObject.SetActive(false);

        availableTileSmokes.Enqueue(tileSmoke);
    }

    #endregion


    #region TimeAnimation
    private TextMeshProUGUI CreateTimeAnimation()
    {
        TextMeshProUGUI timeAnimation = Instantiate(timeAnimationPrefab, timeAnimationParent);
        return timeAnimation;
    }

    public TextMeshProUGUI GetTimeAnimation()
    {
        TextMeshProUGUI timeAnimation;
        if (availableTimeAnimations.Count == 0)
        {
            timeAnimation = CreateTimeAnimation();
        }
        else
        {
            timeAnimation = availableTimeAnimations.Dequeue();
        }
        timeAnimation.gameObject.SetActive(true);
        return timeAnimation;
    }

    public void ReleaseTimeAnimation(TextMeshProUGUI timeAnimation)
    {
        timeAnimation.text = "";
        timeAnimation.gameObject.SetActive(false);

        availableTimeAnimations.Enqueue(timeAnimation);
    }

    #endregion


    #region Explosion

    private ParticleSystem CreateEndGameExplosion()
    {
        ParticleSystem endGameExplosion = Instantiate(endGameExplosionPrefab, endGameExplosionParent);
        return endGameExplosion;
    }

    public ParticleSystem GetEndGameExplosion()
    {
        ParticleSystem endGameExplosion;
        if (availableEndGameExplosions.Count == 0)
        {
            endGameExplosion = CreateEndGameExplosion();
        }
        else
        {
            endGameExplosion = availableEndGameExplosions.Dequeue();
        }
        endGameExplosion.gameObject.SetActive(true);
        return endGameExplosion;
    }

    public void ReleaseEndGameExplosion(ParticleSystem endGameExplosion)
    {
        endGameExplosion.Stop();
        endGameExplosion.gameObject.SetActive(false);

        availableEndGameExplosions.Enqueue(endGameExplosion);
    }

    #endregion
}
