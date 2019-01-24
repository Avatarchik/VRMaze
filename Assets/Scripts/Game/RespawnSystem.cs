﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class which represents a maze position.
 */
public class Position {
    /* Row */
    public int row;
    /* Column */
    public int column;

    /*
     * Constructor
     */
    public Position(int row, int column) {
        this.row = row;
        this.column = column;
    }
}

/*
 * Class which contains all the respawn logic
 */
public class RespawnSystem : MonoBehaviour
{
    /* Flag to indicate if the system is initialized */
    private bool isInitialized = false;
    /* Maze representation */
    private MazeLocation[,] mazeData;
    /* Height of the maze */
    private int rows;
    /* Width of the maze */
    private int columns;
    /* Free positions of the maze */
    private List<Position> freePositions;
    /* Player prefab needed for the instantiate method during the respawn */
    public GameObject playerPrefab;
    /* Enemies prefabs needed for the instantiate method during the respawn */
    public GameObject[] enemiesPrefabs;
    /* Enemies respawn prefab needed for the instantiate method */
    public GameObject enemiesRespawnPrefab;
    /* Final point prefab needed for the instantiate method */
    public GameObject finalPointPrefab;
    /* Quantity of respawn points for enemies */
    private int amountOfEnemyRespawns;
    /* Minimum amount of time to see a new enemy respawning */
    public float minWaitForEnemyRespawn;
    /* Maximum amount of time to see a new enemy respawning */
    public float maxWaitForEnemyRespawn;
    /* Maximum amount of instantiated enemies */
    public float maxEnemies;
    /* Instantiated enemies */
    private List<GameObject> instantiatedEnemies;

    /*
     * Method to "initialize" respawn system. A copy of
     * the maze is needed to provide valid locations
     * for spawning.
     * @param maze Maze data
     */
    public void InitilizeRespawnSystem(MazeLocation[,] maze) {
        mazeData = maze;
        isInitialized = true;
        rows = maze.GetLength(0);
        columns = maze.GetLength(1);
        instantiatedEnemies = new List<GameObject>();

        freePositions = new List<Position>();
        for (int i = 1; i < rows; i++) {
            for (int j = 1; j < columns; ++j) {
                if (mazeData[i, j] == MazeLocation.FREE) {
                    freePositions.Add(new Position(i, j));
                }
            }
        }
        amountOfEnemyRespawns = (int)(freePositions.Count * 0.05f);
    }

    /*
     * Returns a valid (FREE) position
     * @param newMazeLocation Maze location
     * @return Valid position
     */
    private Position GenerateValidPosition(MazeLocation newMazeLocation) {

        if (!isInitialized) {
            throw new System.Exception("Respawn system not initialized");
        }
        Position selectedPosition = freePositions[Random.Range(0, freePositions.Count)];
        mazeData[selectedPosition.row, selectedPosition.column] = newMazeLocation;
        freePositions.Remove(selectedPosition);
        return selectedPosition;
    }

    /*
     * Respawns the player in a valid position of the maze
     */
    public void RespawnPlayer() {
        Position respawnPosition = GenerateValidPosition(MazeLocation.PLAYER_RESPAWN);
        
        Vector3 position = new Vector3(
            respawnPosition.column * MazeMeshGenerator.width, 
            2f, 
            respawnPosition.row * MazeMeshGenerator.width
        );

        Instantiate(playerPrefab, position, Quaternion.identity);
    }

    /*
     * Respawns the enemies in the maze, selects amountOfEnemyRespawns
     * positions as respawns and invokes a coroutine to respawn enemies
     */
    public void RespawnEnemies() {
        List<Vector3> respawnPositions = new List<Vector3>();
        for (int i = 0; i < amountOfEnemyRespawns; ++i) {
            Position position = GenerateValidPosition(MazeLocation.ENEMY_RESPAWN);
            respawnPositions.Add(new Vector3(
                position.column * MazeMeshGenerator.width,
                2f,
                position.row * MazeMeshGenerator.width
            ));
            Instantiate(
                enemiesRespawnPrefab,
                new Vector3(
                    position.column * MazeMeshGenerator.width,
                    0.01f,
                    position.row * MazeMeshGenerator.width
                ),
                Quaternion.identity
            );

        }
        StartCoroutine(RespawnEnemy(respawnPositions));
    }

    /*
     * Coroutine to respawn an enemy in one of the given respawn points
     */
    private IEnumerator RespawnEnemy(List<Vector3> respawnPositions) {
        while (true) {
            yield return new WaitForSeconds(
                Random.Range(
                    minWaitForEnemyRespawn,
                    maxWaitForEnemyRespawn
                )
            );
            if (instantiatedEnemies.Count < maxEnemies) {
                instantiatedEnemies.Add(Instantiate(
                    enemiesPrefabs[Random.Range(0, enemiesPrefabs.Length)],
                    respawnPositions[Random.Range(0, respawnPositions.Count)],
                    Quaternion.identity
                ));
            }
        }
    }

    /*
     * Establish the position of the maze final point
     */
    public void SetFinalPoint() {
        Position finalPointPosition = GenerateValidPosition(MazeLocation.FINAL_POINT);
        Vector3 position = new Vector3(
            finalPointPosition.column * MazeMeshGenerator.width,
            0.01f,
            finalPointPosition.row * MazeMeshGenerator.width
        );
        Instantiate(finalPointPrefab, position, Quaternion.identity);
    }

}
