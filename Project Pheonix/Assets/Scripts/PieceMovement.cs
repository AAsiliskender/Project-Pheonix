
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static CreatePiece;
using static BattleManager;
using static ArmyManager;
using static Targeting;
using static CalcFunctions;

public class PieceMovement : MonoBehaviour
{
    public Tilemap mainTilemap;
    public Tilemap movementTilemap;
    public int debugLevel;

    private Targeting Tgt;

    public struct Move {
        public readonly Vector2Int StartSquare;
        public readonly Vector2Int TargetSquare;
        public readonly Unit Piece;

        public string[] MoveTags;

        // Move constructor
        public Move(Vector2Int startSquare, Vector2Int targetSquare, Unit piece, string[] moveTags = null) // HAVE A MOVE TYPE ADDED LATER
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            Piece = piece;
            MoveTags = moveTags; // Optional tags added to move
            //EliminatedPiece???
        }
    }

    public List<Move> moves;
    private List<Move> allMoves; // All (actual) moves of enemies and allies
    private List<Move> pieceMoves; // All valid moves of piece
    private List<Move> tempPieceMoves; // Move of piece (in function, includes duplicates)
    private List<Move> setPieceMoves; // Move of piece (out of func., actual move) of a specific kind (i.e. active slide)

    public List<Move> hiddenMoves; // Ability moves (not calculated for map control), added and removed


    private Vector2Int pathingPosition;


    private void Awake()
    {
        // Find the Grid GameObject
        GameObject playerInputObj = GameObject.Find("PlayerInput");

        // Get the BattleManager component from the Grid GameObject
        Tgt = playerInputObj.GetComponent<Targeting>();
        if (Tgt == null)
        {
            Debug.Log("Targeting component not found");
            return;
        }

        GameObject mainTilemapObj = GameObject.Find("Tilemap");
        
        mainTilemap = mainTilemapObj.GetComponent<Tilemap>();

    }





    void Start()
    {
        BattleManager BM = GetComponent<BattleManager>(); // MOVE THIS TO BATTLE MANAGER?

        if(debugLevel>0){Debug.Log("First Turn");}

        foreach (Army army in armyList)
        {
            if (army.isOutOfAction) // Guard function
            {
                continue;
            }
            foreach (Unit piece in army.UnitList)
            {
                if (piece.IsOutOfAction) // Guard function
                {
                    continue;
                }

                if (army.Faction == Affiliation.Enemy)
                {
                    Direction relativeRotation = Direction.Backward;
                }
                
            }

        }

        allMoves = GenerateAllMoves();

        BM.TileControlLevels();
    }
    


    List<Move> GenerateAllMoves(){
        // Get player (and also enemy) army
        GameObject GameManagerObj = GameObject.Find("GameManager"); //TODO: MOVE THIS TO A HIGHER ORDER GAMEOBJECT MAYBE EVENT SYSTEM
        ArmyManager AM = GameManagerObj.GetComponent<ArmyManager>();

        
        moves = new List<Move>();
        allMoves = new List<Move>();
        setPieceMoves = new List<Move>();
        
        //TODO: [GENERATE MOVES (BOTH SLIDING, JUMPING AND OF TYPES ATTACK AND MOVE)]
        //TODO: [IF PIECE AFFIL MATCHES TURN, EXECUTABLE MOVE]
        //TODO:  CAN REDUCE THIS TO FOR ALL ARMIES
        foreach (Unit allyPiece in playerArmy.UnitList)
        {
            Debug.Log(allyPiece.UnitName);
            if (allyPiece.IsOutOfAction == false)
            {
                
                setPieceMoves = ActiveJumpingMoves(allyPiece);
                setPieceMoves = ActiveSlidingMoves(allyPiece);
                //setPieceMoves = PassiveSlidingMoves(allyPiece);

                //setPieceMoves.Clear();
                if(debugLevel>1){
                Debug.Log("Adding ally piece moves");
                Debug.Log("Piece move count: " + setPieceMoves.Count);
                }
            }
            
        }

        foreach (Unit enemyPiece in enemyArmy.UnitList)
        {
            Debug.Log(enemyPiece.UnitName);
            if (enemyPiece.IsOutOfAction == false)
            {
                
                setPieceMoves = ActiveJumpingMoves(enemyPiece);
                setPieceMoves = ActiveSlidingMoves(enemyPiece);
                //setPieceMoves = PassiveSlidingMoves(enemyPiece);
                
                if(debugLevel>0){
                Debug.Log("Adding enemy piece moves");
                Debug.Log("Piece move count: " + setPieceMoves.Count);
                }
            }
            
        }

        Debug.Log("Total moves: " + moves.Count);
        
        return moves;
    }

    // Active Sliding (Attack) Moves
    private List<Move> ActiveSlidingMoves(Unit movingPiece)
    {
        //BattleManager BM = new BattleManager();
        BattleManager BM = GetComponent<BattleManager>();
        tempPieceMoves = new List<Move>();

        // Guard Function
        if (movingPiece.ActiveSlideMoveSize == null) // If move size is null, skip
        {
            return tempPieceMoves;
        }

        // For each move direction
        for (int i = 0; i < movingPiece.ActiveSlideMoveSet.Length; i++)
        {
            // Pathing vector of each moveset 
            Vector2Int[] movePathingVector = movingPiece.ActiveSlideMoveSet[i];
            
            // Guard function
            if (movingPiece.ActiveSlideMoveSize == null) // If move size is not null (as should be)
            {
                throw new System.ArgumentException("The move size of a moveset cannot be null!");
                continue;
            }

            // Here we draw the path to check if the target location (for each size) is valid
            Vector2Int moveVect = Vector2Int.zero; 
            
            bool isValid = true;
            //foreach (Vector2Int movePath in movePathingVector)
            
            Vector2Int preMoveVect = SumVector2IntArray(movePathingVector);

            // For each move multiple
            for (int j = 0; j < movingPiece.ActiveSlideMoveSize[i]; j++) // From 1st to nth slide size (for each)
            {
                // Reset Pathing Positions
                pathingPosition = Vector2Int.zero;
                moveVect = Vector2Int.zero;

                // For each pathing                
                for (int k = 0; k < movePathingVector.Length; k++)
                {
                    Vector2Int movePath = movePathingVector[k];
                    
                    moveVect = moveVect + movePath;
                    
                    pathingPosition = movingPiece.Position + moveVect + preMoveVect*j;

                    Dictionary<BattleManager.TileControl,List<Unit>> pathingOccupancy = BM.OccupiedBy(pathingPosition);

                    if (!mainTilemap.HasTile(new Vector3Int(pathingPosition.x,pathingPosition.y,0))) // Target being out of bounds (completely disregarded)
                    {
                        isValid = false;
                        break;

                    } else if (pathingOccupancy.ContainsKey(TileControl.Ally) || pathingOccupancy.ContainsKey(TileControl.Enemy) || pathingOccupancy.ContainsKey(TileControl.Contested)) // If piece on square cannot go further
                    {

                        // If invalid in path search
                        if(debugLevel>2){
                        Debug.Log("Invalid Path: " + pathingPosition); // Need to mark as ally or enemy target - then can add control sqs
                        }
                        isValid = false;
                        break;

                    } else {
                        if(debugLevel>2){
                        Debug.Log("Valid Path: " + pathingPosition);//NOT DONE, FINISH PATHING CODE (CHECK IF COMPLETED)
                        }
                    }

                }

                // If valid at end of path add, else break
                if (isValid) 
                {
                    tempPieceMoves.Add(new Move(movingPiece.Position, pathingPosition, movingPiece, new string[] {"active","slide"}));
                } else {
                    break;
                }

            }
        }
        
        // WILL ADD MOVE ANIMATION LATER
        // NEED TO ADD PIECE ORIENTATION AND TURN SETTING


        // NEED TO ADD MOVE EXECUTION (I.E. RANGED SHOTS OR MELEE ATKS, HOW EXECUTION IS DECIDED)
        // MARKING MOVE AS ACTIVE OR PASSIVE (OR RANGED)

        // Add each valid move to a global move list
        List<Move> actualPieceMoves = ActualMoves(tempPieceMoves);       
        moves.AddRange(actualPieceMoves);

        return tempPieceMoves;
    }

    // Active Jumping (Attack) Moves
    private List<Move> ActiveJumpingMoves(Unit movingPiece)
    {
        //BattleManager BM = new BattleManager();
        BattleManager BM = GetComponent<BattleManager>();
        tempPieceMoves = new List<Move>();

        // Guard Function
        if (movingPiece.ActiveJumpMoveSize == null) // If move size is null, skip
        {
            return tempPieceMoves;
        }

        for (int i = 0; i < movingPiece.ActiveJumpMoveSet.Length; i++)
        {
            Vector2Int moveVector = movingPiece.ActiveJumpMoveSet[i];
            
            // Guard function
            if (movingPiece.ActiveJumpMoveSize == null) // If move size is not null (as should be)
            {
                throw new System.ArgumentException("The move size of a moveset cannot be null!");
                continue;
            }

            for (int j = 1; j <= movingPiece.ActiveJumpMoveSize[i]; j++) // From 1st to nth jump (for each)
            {

                Vector2Int targetLocation = movingPiece.Position+(moveVector*j);
                
                Dictionary<BattleManager.TileControl,List<Unit>> occupancy = BM.OccupiedBy(targetLocation);

                if (!mainTilemap.HasTile(new Vector3Int(targetLocation.x,targetLocation.y,0))) // Target being out of bounds (completely disregarded and j cannot increase further)
                {
                    break;
                } else if (occupancy.ContainsKey(TileControl.Ally) || occupancy.ContainsKey(TileControl.Enemy) || occupancy.ContainsKey(TileControl.Contested)) // If piece on square cannot go further
                {

                    //Debug.Log("Adding move on filled square: " + targetLocation); // Need to mark as ally or enemy target - then can add control sqs
                    
                    tempPieceMoves.Add(new Move(movingPiece.Position, targetLocation, movingPiece, new string[] {"active","jump"}));
                    break;

                } else {
                    //Debug.Log("Adding move on empty square: " + targetLocation);
                    
                    tempPieceMoves.Add(new Move(movingPiece.Position, targetLocation, movingPiece, new string[] {"active","jump"}));
                }
            }
        }

        // NEED TO ADD MOVE EXECUTION (I.E. RANGED SHOTS OR MELEE ATKS, HOW EXECUTION IS DECIDED)
        // MARKING MOVE AS ACTIVE OR PASSIVE (OR RANGED)

        // Add each valid move to a global move list
        List<Move> actualPieceMoves = ActualMoves(tempPieceMoves);       
        moves.AddRange(actualPieceMoves);


        return tempPieceMoves;
    }

    // Passive Sliding (Defensive) Moves
    private List<Move> PassiveSlidingMoves(Unit movingPiece)
    {
        //BattleManager BM = new BattleManager();
        BattleManager BM = GetComponent<BattleManager>();
        tempPieceMoves = new List<Move>();

        // Guard Function
        if (movingPiece.PassiveSlideMoveSize == null) // If move size is null, skip
        {
            return tempPieceMoves;
        }

        // For each move direction
        for (int i = 0; i < movingPiece.PassiveSlideMoveSet.Length; i++)
        {
            Debug.Log(movingPiece.PassiveSlideMoveSet.Length);
            // Pathing vector of each moveset 
            Vector2Int[] movePathingVector = movingPiece.PassiveSlideMoveSet[i];
            
            // Guard function
            if (movingPiece.PassiveSlideMoveSize == null) // If move size is not null (as should be)
            {
                throw new System.ArgumentException("The move size of a moveset cannot be null!");
                continue;
            }

            // Here we draw the path to check if the target location (for each size) is valid
            Vector2Int moveVect = Vector2Int.zero; 
            
            bool isValid = true;
            //foreach (Vector2Int movePath in movePathingVector)
            
            Vector2Int preMoveVect = SumVector2IntArray(movePathingVector);

            // For each move multiple
            for (int j = 0; j < movingPiece.PassiveSlideMoveSize[i]; j++) // From 1st to nth slide size (for each)
            {
                // Reset Pathing Positions
                pathingPosition = Vector2Int.zero;
                moveVect = Vector2Int.zero;

                // For each pathing                
                for (int k = 0; k < movePathingVector.Length; k++)
                {
                    Vector2Int movePath = movePathingVector[k];
                    
                    moveVect = moveVect + movePath;
                    
                    pathingPosition = movingPiece.Position + moveVect + preMoveVect*j;

                    Dictionary<BattleManager.TileControl,List<Unit>> pathingOccupancy = BM.OccupiedBy(pathingPosition);

                    if (!mainTilemap.HasTile(new Vector3Int(pathingPosition.x,pathingPosition.y,0))) // Target being out of bounds (completely disregarded)
                    {
                        isValid = false;
                        break;

                    } else if (pathingOccupancy.ContainsKey(TileControl.Ally) || pathingOccupancy.ContainsKey(TileControl.Enemy) || pathingOccupancy.ContainsKey(TileControl.Contested)) // If piece on square cannot go further
                    {

                        // If invalid in path search
                        if(debugLevel>2){
                        Debug.Log("Invalid Path: " + pathingPosition); // Need to mark as ally or enemy target - then can add control sqs
                        }
                        isValid = false;
                        break;

                    } else {
                        if(debugLevel>2){
                        Debug.Log("Valid Path: " + pathingPosition);//NOT DONE, FINISH PATHING CODE
                        }
                    }

                }

                // If valid at end of path add, else break
                if (isValid) 
                {
                    tempPieceMoves.Add(new Move(movingPiece.Position, pathingPosition, movingPiece, new string[] {"passive","slide"}));
                } else {
                    break;
                }

            }
        }
        
        // WILL ADD MOVE VISUALISER LATER
        // ALLMOVES HAVE ACTIVE AND PASSIVE
        // 
        // NEED TO ADD PIECE ORIENTATION AND TURN SETTING (MAYBE SMTH LIKE TURN % X WITH X BEING PLAYER COUNTS)


        // NEED TO ADD MOVE EXECUTION (I.E. RANGED SHOTS OR MELEE ATKS, HOW EXECUTION IS DECIDED)
        // MARKING MOVE AS ACTIVE OR PASSIVE (OR RANGED)

        // Add each valid move to a global move list
        List<Move> actualPieceMoves = ActualMoves(tempPieceMoves);       
        moves.AddRange(actualPieceMoves);
        
        return tempPieceMoves;
    }

    // Passive Jumping (Defensive) Moves (NOT DONE)
    private void PassiveJumpingMoves()
    {
        
    }

    // Ranged (Aggressive) Attacks (Counts as Moves) (NOT DONE)
    private void RangedMoves()
    {
        
    }



    // Using specific moves/target locations
    public List<Move> SeekMovesOf(Unit piece)
    {
        pieceMoves = new List<Move>();

        Debug.Log("Seeking moves of piece: " + piece.UnitName);


        setPieceMoves = ActiveJumpingMoves(piece);
        pieceMoves.AddRange(setPieceMoves);   

        setPieceMoves = ActiveSlidingMoves(piece);
        pieceMoves.AddRange(setPieceMoves);

        //setPieceMoves = PassiveJumpingMoves(piece);
        //pieceMoves.AddRange(setPieceMoves);

        //setPieceMoves = PassiveSlidingMoves(piece);
        //pieceMoves.AddRange(setPieceMoves);

        //setPieceMoves = RangedMoves(piece);
        //pieceMoves.AddRange(setPieceMoves);

        return pieceMoves;
    }


    // Filters moves to remove duplicates (such as from multiple pathing) and adds to movesActual for control marking
    public List<Move> ActualMoves(List<Move> candidateMoveSet)
    {
        List<Move> selectedMoves = new List<Move>();
        HashSet<Vector2Int> candidateLocations = new HashSet<Vector2Int>();

        foreach (Move candidateMove in candidateMoveSet)
        {
            if (!candidateLocations.Contains(candidateMove.TargetSquare))
            {
                // Add location if not visited in set (and add move to a list)
                candidateLocations.Add(candidateMove.TargetSquare);
                selectedMoves.Add(candidateMove);
            }
            // Else skip this location as it has already been visited
        }
        return selectedMoves;
    }

    public void MovePieceTo(Unit movingPiece, Vector3Int destinationCell, bool isMoveDeclared = false)
    {
        if(!Tgt.moveLocationList.Contains(destinationCell))
        {
            return;
        }

        BattleManager BM = GetComponent<BattleManager>();

        //GameObject objMovingPiece = GameObject.Find(movingPiece.UnitName);

        //Transform transformMovingPiece = movingPWiece.ThisGameObject.transform;

        Vector3 movingPieceRenderDestination = new Vector3(destinationCell.x+0.5f, 0, destinationCell.y+0.5f);

        movingPiece.ThisGameObject.transform.position = movingPieceRenderDestination;

        movingPiece.Position = new Vector2Int (destinationCell.x,destinationCell.y);

        // Record move (as done)
        if (!isMoveDeclared) // Declare the move if it hasn't been done (before), but add tag to not repeat this function
        {
            // Find move being committed (matching piece, then target location)
            foreach (Move move in allMoves)
            {
                if (move.Piece.UnitName == movingPiece.UnitName)
                {
                    if (movingPiece.Position == move.TargetSquare)
                    {
                        BM.CommitMove(move, true);
                    }


                }
            }

        }
        


        // Clear previous move availability mainTilemap as movement done.
        foreach (Vector3Int savedMoveLocation in Tgt.savedMoveLocationList)
        {
            Tgt.TileClear(savedMoveLocation, movementTilemap);
        }
        Tgt.savedMoveLocationList.Clear();
        
    }

    // Actively changes the available moves for a piece
    // Change the moves depending on the relations between the orientations, i.e rotates the piece
    public static void ChangeOrientation(Unit piece, Direction rotation)
    {
        // Nested array matrices are column, row i.e. [2,3] is {{x1,x2,x3},{y1,y2,y3}} and 1st vector is x1,y1
        // Multiplication is done on [i] vector pt, matrix[j,i] vector[j] over all j
        
        // Note 'forward' matrix is the identity matrix and does nothing
        int[,] rotationMatrix = GetRotateMatrix(rotation);


        if (piece.ActiveJumpMoveSet != null)
        {
            for (int i = 0; i < piece.ActiveJumpMoveSet.Count(); i++)
            {
                Vector2Int activeMove = piece.ActiveJumpMoveSet[i];
                activeMove = VectorMatrixMult(rotationMatrix,activeMove);
                piece.ActiveJumpMoveSet[i] = activeMove;
            }
        }
        
        if (piece.PassiveJumpMoveSet != null) // MAKE SURE THIS WORKS
        {
            for (int i = 0; i < piece.PassiveJumpMoveSet.Count(); i++)
            {
                Vector2Int passiveMove = piece.PassiveJumpMoveSet[i];
                passiveMove = VectorMatrixMult(rotationMatrix,passiveMove);
                piece.PassiveJumpMoveSet[i] = passiveMove;
            }
        }
        
        ///for (int i = 0; i < piece.ActiveSlideMoveSet.Count(); i++) // TODO: FIX THIS ISSUE HEHE
        //{
            
        //    for (int j = 0; j < piece.ActiveSlideMoveSet.Count(); i++) // TODO: FIX THIS ISSUE HEHE
        //    {
        //        Vector2Int activeMove = piece.ActiveSlideMoveSet[i][j];
        //        activeMove = VectorMatrixMult(finalOrientMatrix,activeMove);
        //        piece.ActiveSlideMoveSet[i][j] = activeMove;
        //    }


        //}
        //for (int i = 0; i < piece.PassiveSlideMoveSet.Count(); i++)
        //{
        //    for (int j = 0; j < piece.PassiveSlideMoveSet.Count(); i++) // TODO: FIX THIS ISSUE HEHE
        //    {
        //        Vector2Int passiveMove = piece.PassiveSlideMoveSet[i][j];
        //        passiveMove = VectorMatrixMult(finalOrientMatrix,passiveMove);
        //        piece.PassiveSlideMoveSet[i][j] = passiveMove;
        //    }
            
        //}


        piece.Orientation = GetNewOrientation(piece, rotation);

        return;
    }

    // Actively changes the available moves for a piece
    // Change the moves depending on the desired direction (sets the ultimate direction, rather than rotating it)
    public static void SetOrientation(Unit piece, Direction newOrientation)
    {
        // FINISH THIS CODE
        if(piece.Orientation == newOrientation) // Guard function
        {
            return;
        }

        int originalNumber = OrientToNumber(piece.Orientation);
        int newOrientNumber = OrientToNumber(newOrientation);
        int rotationNumber = (newOrientNumber - originalNumber + 4)%4;


        // Nested array matrices are column, row i.e. [2,3] is {{x1,x2,x3},{y1,y2,y3}} and 1st vector is x1,y1
        // Multiplication is done on [i] vector pt, matrix[j,i] vector[j] over all j
        // Note 'forward' matrix is the identity matrix and does nothing
        Direction rotation = NumberToOrient(rotationNumber);
        int[,] rotationMatrix = GetRotateMatrix(rotation);


        if (piece.ActiveJumpMoveSet != null)
        {
            for (int i = 0; i < piece.ActiveJumpMoveSet.Count(); i++)
            {
                Vector2Int activeMove = piece.ActiveJumpMoveSet[i];
                activeMove = VectorMatrixMult(rotationMatrix,activeMove);
                piece.ActiveJumpMoveSet[i] = activeMove;
            }
        }
        
        if (piece.PassiveJumpMoveSet != null) // TODO: MAKE SURE THIS WORKS
        {
            for (int i = 0; i < piece.PassiveJumpMoveSet.Count(); i++)
            {
                Vector2Int passiveMove = piece.PassiveJumpMoveSet[i];
                passiveMove = VectorMatrixMult(rotationMatrix,passiveMove);
                piece.PassiveJumpMoveSet[i] = passiveMove;
            }
        }
        
        ///for (int i = 0; i < piece.ActiveSlideMoveSet.Count(); i++) // TODO: FIX THIS ISSUE HEHE
        //{
            
        //    for (int j = 0; j < piece.ActiveSlideMoveSet.Count(); i++) // TODO: FIX THIS ISSUE HEHE
        //    {
        //        Vector2Int activeMove = piece.ActiveSlideMoveSet[i][j];
        //        activeMove = VectorMatrixMult(finalOrientMatrix,activeMove);
        //        piece.ActiveSlideMoveSet[i][j] = activeMove;
        //    }


        //}
        //for (int i = 0; i < piece.PassiveSlideMoveSet.Count(); i++)
        //{
        //    for (int j = 0; j < piece.PassiveSlideMoveSet.Count(); i++) // TODO: FIX THIS ISSUE HEHE
        //    {
        //        Vector2Int passiveMove = piece.PassiveSlideMoveSet[i][j];
        //        passiveMove = VectorMatrixMult(finalOrientMatrix,passiveMove);
        //        piece.PassiveSlideMoveSet[i][j] = passiveMove;
        //    }
            
        //}


        piece.Orientation = newOrientation;

        return;
    }
    
    // Sets matrix to its correct rotation matrix;
    public static int[,] GetRotateMatrix(Direction rotation)
    {
        int[,] rotateMatrix = new int[2,2];

        if (rotation == Direction.Forward) // This does nothing
        {
            rotateMatrix = new int[2,2] {{1, 0}, {0, 1}};

        } else if (rotation == Direction.Backward) {
            rotateMatrix = new int[2,2] {{-1, 0}, {0, -1}};

        } else if (rotation == Direction.Rightward) {
            rotateMatrix = new int[2,2] {{0, -1}, {1, 0}};

        } else if (rotation == Direction.Leftward) {
            rotateMatrix = new int[2,2] {{0, 1}, {-1, 0}};

        }
        return rotateMatrix;
    }

    // Gets the (new) orientation of a piece (based on a given rotation)
    public static Direction GetNewOrientation(Unit piece, Direction rotation)
    {
        int originalNumber = OrientToNumber(piece.Orientation);
        int rotationNumber = OrientToNumber(rotation);

        int newOrientationNumber = (originalNumber + rotationNumber)%4;

        Direction newOrientation = NumberToOrient(newOrientationNumber);

        return newOrientation;
    }


    // Will do this using moduli (periodicity), 0 is forward, and goes clockwise right, back, left...
    public static int OrientToNumber(Direction orient)
    {
        if (orient == Direction.Rightward)
        {
            return 1;
        } else if (orient == Direction.Backward)
        {
            return 2;
        } else if (orient == Direction.Leftward)
        {
            return 3;
        } else {
            return 0;
        }
    }

    public static Direction NumberToOrient(int number)
    {
        if (number == 1)
        {
            return Direction.Rightward;
        } else if (number == 2)
        {
            return Direction.Backward;
        } else if (number == 3)
        {
            return Direction.Leftward;
        } else {
            return Direction.Forward;
        }
    }
}
