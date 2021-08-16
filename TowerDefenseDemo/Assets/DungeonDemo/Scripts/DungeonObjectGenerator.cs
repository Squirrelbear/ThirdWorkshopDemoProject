using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// See for code reference:
// http://gregschlom.com/devlog/2014/06/29/Poisson-disc-sampling-Unity.html
public class DungeonObjectGenerator : MonoBehaviour
{
    public int ignoreIDsAndLower = 0;
    public bool populateUsingFiltered = true;
    public float poissonRadius = 2f;

    public DungeonManager dungeonManager;

    private const int k = 30;  // Maximum number of attempts before marking a sample as inactive.

    private Rect rect;
    private float radius2;  // radius squared
    private float cellSize;
    private Vector2[,] grid;
    private List<Vector2> activeSamples = new List<Vector2>();
    private int width, height;

    public List<Vector2> sampleResult;
    private int[,] demoGrid;
    private int[,] filteredGrid;
    public List<Vector2Int> filteredResult;

    public void spawnObjects()
    {
        if(dungeonManager == null)
        {
            dungeonManager = DungeonManager.Instance;
        }

        if (populateUsingFiltered)
            spawnUsingFiltered();
        else
            spawnUsingUnFiltered();
    }

    private void spawnUsingFiltered()
    {
        foreach(Vector2Int pos in filteredResult)
        {
            int randID = Random.Range(0, dungeonManager.objectTextures.Count);
            dungeonManager.createObject(randID, pos);
        }
    }

    private void spawnUsingUnFiltered()
    {
        foreach (Vector2 pos in sampleResult)
        {
            int randID = Random.Range(0, dungeonManager.objectTextures.Count);
            dungeonManager.createObject(randID, new Vector2Int((int)pos.x, (int)pos.y));
        }
    }

    public List<Vector2> generatePoisson(int width, int height)
    {
        return generatePoisson(width, height, poissonRadius);
    }

    /// Create a sampler with the following parameters:
    ///
    /// width:  each sample's x coordinate will be between [0, width]
    /// height: each sample's y coordinate will be between [0, height]
    /// radius: each sample will be at least `radius` units away from any other sample, and at most 2 * `radius`.
    public List<Vector2> generatePoisson(int width, int height, float radius)
    {
        this.width = width;
        this.height = height;
        rect = new Rect(0, 0, width, height);
        radius2 = radius * radius;
        cellSize = radius / Mathf.Sqrt(2);
        grid = new Vector2[Mathf.CeilToInt(width / cellSize),
                           Mathf.CeilToInt(height / cellSize)];

        sampleResult = new List<Vector2>();
        demoGrid = new int[width, height];
        filteredGrid = new int[width, height];
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                demoGrid[x, y] = 0;
                filteredGrid[x, y] = 0;
            }
        }

        foreach (Vector2 sample in Samples())
        {
            sampleResult.Add(sample);
            demoGrid[(int)sample.x, (int)sample.y] = 1;
        }

        return sampleResult;
    }

    public void printGrid()
    {
        string result = "NOT Filtered Poisson Grid:\n";
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result += demoGrid[x, y];
            }
            result += "\n";
        }
        Debug.Log(result);
    }

    public List<Vector2Int> filterGrid(int[,] mapData)
    {
        int removedPoints = 0;
        int retainedPoints = 0;

        filteredResult = new List<Vector2Int>();
        foreach(Vector2 pos in sampleResult)
        {
            Vector2Int fixedPos = new Vector2Int((int)pos.x, (int)pos.y);
            if(mapData[fixedPos.x, fixedPos.y] <= ignoreIDsAndLower)
            {
                removedPoints++;
            } else
            {
                retainedPoints++;
                filteredResult.Add(fixedPos);
                filteredGrid[fixedPos.x, fixedPos.y] = 1;
            }
        }
        
        Debug.Log("Filtered Poisson. Removed: " + removedPoints + " Retained: " + retainedPoints);
        return filteredResult;
    }

    public void printFilteredGrid()
    {
        string result = "Filtered Poisson Grid:\n";
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result += demoGrid[x, y];
            }
            result += "\n";
        }
        Debug.Log(result);
    }

    /// Return a lazy sequence of samples. You typically want to call this in a foreach loop, like so:
    ///   foreach (Vector2 sample in sampler.Samples()) { ... }
    public IEnumerable<Vector2> Samples()
    {
        // First sample is choosen randomly
        yield return AddSample(new Vector2(Random.value * rect.width, Random.value * rect.height));

        while (activeSamples.Count > 0)
        {

            // Pick a random active sample
            int i = (int)Random.value * activeSamples.Count;
            Vector2 sample = activeSamples[i];

            // Try `k` random candidates between [radius, 2 * radius] from that sample.
            bool found = false;
            for (int j = 0; j < k; ++j)
            {

                float angle = 2 * Mathf.PI * Random.value;
                float r = Mathf.Sqrt(Random.value * 3 * radius2 + radius2); // See: http://stackoverflow.com/questions/9048095/create-random-number-within-an-annulus/9048443#9048443
                Vector2 candidate = sample + r * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Accept candidates if it's inside the rect and farther than 2 * radius to any existing sample.
                if (rect.Contains(candidate) && IsFarEnough(candidate))
                {
                    found = true;
                    yield return AddSample(candidate);
                    break;
                }
            }

            // If we couldn't find a valid candidate after k attempts, remove this sample from the active samples queue
            if (!found)
            {
                activeSamples[i] = activeSamples[activeSamples.Count - 1];
                activeSamples.RemoveAt(activeSamples.Count - 1);
            }
        }
    }

    private bool IsFarEnough(Vector2 sample)
    {
        GridPos pos = new GridPos(sample, cellSize);

        int xmin = Mathf.Max(pos.x - 2, 0);
        int ymin = Mathf.Max(pos.y - 2, 0);
        int xmax = Mathf.Min(pos.x + 2, grid.GetLength(0) - 1);
        int ymax = Mathf.Min(pos.y + 2, grid.GetLength(1) - 1);

        for (int y = ymin; y <= ymax; y++)
        {
            for (int x = xmin; x <= xmax; x++)
            {
                Vector2 s = grid[x, y];
                if (s != Vector2.zero)
                {
                    Vector2 d = s - sample;
                    if (d.x * d.x + d.y * d.y < radius2) return false;
                }
            }
        }

        return true;

        // Note: we use the zero vector to denote an unfilled cell in the grid. This means that if we were
        // to randomly pick (0, 0) as a sample, it would be ignored for the purposes of proximity-testing
        // and we might end up with another sample too close from (0, 0). This is a very minor issue.
    }

    /// Adds the sample to the active samples queue and the grid before returning it
    private Vector2 AddSample(Vector2 sample)
    {
        activeSamples.Add(sample);
        GridPos pos = new GridPos(sample, cellSize);
        grid[pos.x, pos.y] = sample;
        return sample;
    }

    /// Helper struct to calculate the x and y indices of a sample in the grid
    private struct GridPos
    {
        public int x;
        public int y;

        public GridPos(Vector2 sample, float cellSize)
        {
            x = (int)(sample.x / cellSize);
            y = (int)(sample.y / cellSize);
        }
    }
}
