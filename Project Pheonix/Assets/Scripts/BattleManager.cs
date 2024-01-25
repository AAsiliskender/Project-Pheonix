using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
using static CreatePiece;
using static PieceMovement;
using static ArmyManager;
using static CalcFunctions;
using static GameManager;

public class BattleManager : MonoBehaviour
{
    public int debugLevel;

    public Tilemap tilemap;
    private Tilemap allyControlTilemap;
    private Tilemap enemyControlTilemap;
    public TileBase[] defControlTileBase;
    public TileBase[] atkControlTileBase;

    // Tile control levels:
    private int[] controlStatus; 

    // Determines tile
    public enum TileControl
    {
        Ally, // Includes Player (for now)
        Enemy,
        Contested,
        Empty
    }

    // Turn variables:
    private int allyTurnTokens;
    private int enemyTurnTokens;

    // X, Y location and levels of control (ally, enemy)
    public Dictionary<Vector2Int,int[]> controlMap;

    // Location on tilemap (with z) and list of units on tile
    public Dictionary<Vector3Int, List<Unit>> mapOfTileAndUnits = new Dictionary<Vector3Int, List<Unit>>();


    // Armies
    public static Army PlayerArmy;
    public static Army EnemyArmy;

    public static bool inBattle;

    // Turn of:
    public static int turn; // Nominal turn
    public static Army turnOf;

    // Internal turn states:
    public static List<int> preMoveTotal; // Pre turn move (for all turns)
    public static List<int> postMoveTotal; // Post turn move (for all turns)
    public static int instantMoveCount; // Number of instant moves so far in this turn 
    public static int internalTurnCounter; // Counts the internal turn state (accounting for instant actions)
    public static List<List<Move>> moveQueue; // The list of moves queued up (all turns), sorted later

    public static string fullTurnSequence; // Real turn sequence including pre/posts "--2", "4+" etc. and instant moves (that have happened)
    public static string currentTurnState; // Real turn state including pre/posts "--2", "4+" etc. and instant moves (that have happened)
    public static List<string> allRealTurns; // Includes all pre-turn and post-turn moves, "0", "1", "-2", "2", "2+", "2++", used for records, past, present and (forseeable) future
    

    // EVENTS (try this for moves):
    public delegate void MoveEvent(Move move);
    public event MoveEvent MoveCommit;

    // GameObjects
    private GameObject factionObj;
    

    private Targeting Tgt;

    private void Awake()
    {
        // FOR NOW WE ONLY CREATE PRE-SET UNITS
        //PieceManager PM = GetComponent<PieceManager>();
        

        

    }

    void OnEnable()
    {
        gamePaused = false;
        inBattle = true;
        // On an action:
        //EventManager.BattleActionTaken += dfsdf; // Record the map/battle state
        //EventManager.BattleActionTaken += SDFS; // Refresh map and move sets

    }

    void OnDisable()
    {
        inBattle = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        // HERE WE PLAN TO ADD PIECES ONTO BOARD BEFORE WE EDIT PRE-BATTLE (UNLESS AMBUSH)
        // for now we add pre-determined pieces onto board
        tilemap.GetComponent<Tilemap>();

        // Create File for tests
        // Testing Function
        //bool test = TestMatrixMult();
        //Debug.Log("Matrix multiplication test passed: " + test);
        bool test = TestVectorMatrixMult();
        Debug.Log("Matrix-Vector multiplication test passed: " + test);

        PieceManager PM = GetComponent<PieceManager>();
        PieceMovement PMove = GetComponent<PieceMovement>();

        // DETERMINE OR ADD TURN, WE START AS Turn=0   
        turn = 0;
        preMoveTotal = new List<int>();
        postMoveTotal = new List<int>();
        preMoveTotal.Add(0);
        postMoveTotal.Add(0);

        // Create army GameObjects (if in action) and we will add pieces to it
        foreach (Army army in armyList) 
        {
            if (army.isOutOfAction) // Guard function
            {
                continue;
            }
            // Create army, (child) pieces, (grandchildren) on field and off field
            GameObject armyPiecesObj = new GameObject("Pieces");
            GameObject armyOnFieldObj = new GameObject("On_Field");
            GameObject armyOffFieldObj = new GameObject("Off_Field");
            // Put the correct dependencies (player->army->pieces->on/off_field)
            if (army.Faction == Affiliation.Player)
            {
                factionObj = GameObject.Find("Player").gameObject;
            } else if (army.Faction == Affiliation.Ally)
            {
                factionObj = GameObject.Find("Ally").gameObject;
            } else if (army.Faction == Affiliation.Enemy)
            {
                factionObj = GameObject.Find("Enemy").gameObject;
            }
            army.ThisGameObject.transform.parent = factionObj.transform;
            armyPiecesObj.transform.parent = army.ThisGameObject.transform;
            armyOnFieldObj.transform.parent = armyPiecesObj.transform;
            armyOffFieldObj.transform.parent = armyPiecesObj.transform;

            // Debug data:
            if(debugLevel>1){Debug.Log("Created Army GameObject: " + army.ArmyName.ToString());}           
        }
        GameObject PlayerArmyObj = GameObject.Find("PlayerArmy"); //GENERALISE THIS
        //ArmyManager AM = PlayerArmyObj.GetComponent<ArmyManager>();

        //if (AM == null)
        //{
        //    Debug.Log("ArmyManager component not found");
        //    return;
        //}

        if (debugLevel > 0)
        {   
            Debug.Log("Player Army Size: " + playerArmy.UnitList.Count.ToString());
            //Debug.Log("Ally Army Size: " + AllyArmy1.UnitList.Count.ToString());
            Debug.Log("Enemy Army Size: " + enemyArmy.UnitList.Count.ToString());
        }
        

        

        
        //
        PM.AddPiece(playerArmy.UnitList[0], new Vector2Int(2,-3) );
        PM.AddPiece(playerArmy.UnitList[1], new Vector2Int(3,-3) );
        PM.AddPiece(playerArmy.UnitList[2], new Vector2Int(4,-3) );
        PM.AddPiece(playerArmy.UnitList[3], new Vector2Int(3,-4) );

        PM.AddPiece(enemyArmy.UnitList[0], new Vector2Int(-1,2) );
        PM.AddPiece(enemyArmy.UnitList[1], new Vector2Int(-4,2) );
        PM.AddPiece(enemyArmy.UnitList[2], new Vector2Int(2,2) );
        PM.AddPiece(enemyArmy.UnitList[3], new Vector2Int(3,3) );
        
        


        // Add each unit (if in action) to map of tile and units //REDUCE USING ALL ARMY CODE
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
                    Direction newOrientation = Direction.Backward;
                    PieceMovement.SetOrientation(piece,newOrientation);
                }
                if (army.Faction == Affiliation.Player)
                {
                    Direction newOrientation = Direction.Leftward;
                    //PieceMovement.SetOrientation(piece,newOrientation);
                }
                

                if (debugLevel > 1) {Debug.Log( "Added " + piece.AttachedArmy.Faction.ToString() + " Piece onto map at: " + new Vector3Int(piece.Position.x, piece.Position.y, 0));}
                UnitIsOnTile(piece, new Vector3Int(piece.Position.x, piece.Position.y, 0));
            }

        }

        foreach (Unit allyPiece in playerArmy.UnitList)
        {
            if (!allyPiece.IsOutOfAction)
            {
                if (debugLevel > 1) {Debug.Log( "Added Ally Piece onto map at: " + new Vector3Int(allyPiece.Position.x, allyPiece.Position.y, 0));}
                
                UnitIsOnTile(allyPiece, new Vector3Int(allyPiece.Position.x, allyPiece.Position.y, 0));
            }
        }

        //foreach (Unit allyPiece in ArmyManager.AllyUnitList)
        //{
        //    if (!allyPiece.IsOutOfAction)
        //    {
        //        if (debugLevel > 1) {Debug.Log( "Added Ally onto map at: " + new Vector3Int(allyPiece.Position.x, allyPiece.Position.y, 0));}
        //        
        //        UnitIsOnTile(allyPiece, new Vector3Int(allyPiece.Position.x, allyPiece.Position.y, 0));
        //    }
        //}
        
        foreach (Unit enemyPiece in enemyArmy.UnitList)
        {
            if (!enemyPiece.IsOutOfAction)
            {
                if (debugLevel > 1) {Debug.Log( "Added Enemy onto map at: " + new Vector3Int(enemyPiece.Position.x, enemyPiece.Position.y, 0));}

                UnitIsOnTile(enemyPiece, new Vector3Int(enemyPiece.Position.x, enemyPiece.Position.y, 0));
            }
        }
        //MUST CLEAR MAP OF TILE AND UNITS EVERY TURN
        
        // Set the attacker and ambushers
        SetAttacker(playerArmy);

        // Calculates and set's turn to relevant army.
        turnOf = TurnCalculator(playerArmy, enemyArmy,turn); // MIGHT NEED TO ADD FUNCTIONALITY FOR MULTIPLE ARMIES;
        Debug.Log("It is turn of: " + turnOf.ArmyName);

        // CODE TO BEGIN TURN 0 HERE

    }


    // Functions
    

    //List<Vector3Int, List<Unit>, int> BattleHistory = new List<Vector3Int, List<Unit>, int>();
    Dictionary<Dictionary<Vector3Int, List<Unit>>, int> BattleHistory = new Dictionary<Dictionary<Vector3Int, List<Unit>>, int>();
    Dictionary <int, Move> TurnHistory = new Dictionary <int, Move>(); // NOT CONFIRM

    // Add a unit to a tile position (x, y, z) in the dictionary
    public void UnitIsOnTile(Unit unit, Vector3Int pos)
    {
        if (!mapOfTileAndUnits.ContainsKey(pos)) {
            mapOfTileAndUnits[pos] = new List<Unit>();
        }
        mapOfTileAndUnits[pos].Add(unit);

    }

    // Get a list of units on a tile at position (x, y, z) from dictionary
    public List<Unit> GetUnitsFromTile(Vector3Int pos)
    {

        if (!mapOfTileAndUnits.ContainsKey(pos)) {
            return new List<Unit>();
        }
        return mapOfTileAndUnits[pos];
    }

    // Every turn, this code will cycle through tilepieces and if has tile in that
    // location, will record occupancy (by unitID), also records it per turn.
    public void MapStateRecord(Move move)
    {
        


        //Debug.Log(realTurnSequence);

        //BattleHistory.Add(mapOfTileAndUnits, turn);
        TurnHistory.Add(turn, move);



        
    }

    public void TileControlLevels()
    {
        PieceMovement PMove = GetComponent<PieceMovement>();
        GameObject allyControlTilemapObj = GameObject.Find("Tilemap (Control, Ally)");
        GameObject enemyControlTilemapObj = GameObject.Find("Tilemap (Control, Enemy)");
        allyControlTilemap = allyControlTilemapObj.GetComponent<Tilemap>();
        enemyControlTilemap = enemyControlTilemapObj.GetComponent<Tilemap>();

        controlMap = new Dictionary<Vector2Int,int[]>();
        foreach (Move movement in PMove.moves)
        {
            
            // If no tile on control map, add tile and change value, else change value
            if(!controlMap.ContainsKey(movement.TargetSquare))
            {
                controlStatus = new int[] {0,0};
                controlMap.Add(movement.TargetSquare,controlStatus);

                if(movement.Piece.AttachedArmy.Faction == Affiliation.Ally || movement.Piece.AttachedArmy.Faction == Affiliation.Player)
                {
                    controlMap[movement.TargetSquare][0]++;
                }
                else if (movement.Piece.AttachedArmy.Faction == Affiliation.Enemy)
                {
                    controlMap[movement.TargetSquare][1]++;
                }
            }
            else
            {
                if(movement.Piece.AttachedArmy.Faction == Affiliation.Ally || movement.Piece.AttachedArmy.Faction == Affiliation.Player)
                {
                    controlMap[movement.TargetSquare][0]++;
                }
                else if (movement.Piece.AttachedArmy.Faction == Affiliation.Enemy)
                {
                    controlMap[movement.TargetSquare][1]++;
                }

            }
        }

        // Set tile based on control
        foreach (Vector2Int controlTileCheck in controlMap.Keys)
        {
            if (controlTileCheck[0]>5)
            {
                allyControlTilemap.SetTile(new Vector3Int(controlTileCheck.x,controlTileCheck.y,0), defControlTileBase[ 5 ]);
            }
            else
            {
                allyControlTilemap.SetTile(new Vector3Int(controlTileCheck.x,controlTileCheck.y,0), defControlTileBase[ controlMap[controlTileCheck][0] ]);
            }

            if (controlTileCheck[1]>5)
            {
                enemyControlTilemap.SetTile(new Vector3Int(controlTileCheck.x,controlTileCheck.y,0), atkControlTileBase[ 5 ] );
            }
            else
            {
                enemyControlTilemap.SetTile(new Vector3Int(controlTileCheck.x,controlTileCheck.y,0), atkControlTileBase[ controlMap[controlTileCheck][1]  ] );
            }


            //enemyControlTilemap.SetTile(new Vector3Int(controlTileCheck.x,controlTileCheck.y,0), atkControlTileBase[ controlTileCheck[1]] );
            
        }
    }


    // Shows the list of units on a tile and who it's controlled by
    public Dictionary<TileControl,List<Unit>> OccupiedBy(Vector2Int checkedPosition)
    {
        // When called, returns Units that occupy checked tile
        List<Unit> unitListOnTile = GetUnitsFromTile(new Vector3Int (checkedPosition.x,checkedPosition.y,0)); // Get list of units in a tile
        Debug.Log("Checking tile " + checkedPosition);

        bool allyOnTile = false;
        bool enemyOnTile = false;
        int allyCount = 0;
        int enemyCount = 0;

        foreach(Unit unitOnTile in unitListOnTile)
        {
            allyOnTile = false;
            enemyOnTile = false;

            if(unitOnTile.AttachedArmy.Faction == Affiliation.Player) // Currently ally and player units contribute to the same count 
            {
                allyOnTile = true;
                allyCount++;
                Debug.Log("Selected Player Unit " + unitOnTile.UnitName + " is on tile " + unitOnTile.Position);
            }

            if(unitOnTile.AttachedArmy.Faction == Affiliation.Ally)
            {
                allyOnTile = true;
                allyCount++;
                Debug.Log("Selected Ally Unit " + unitOnTile.UnitName + " is on tile " + unitOnTile.Position);
            }

            if(unitOnTile.AttachedArmy.Faction == Affiliation.Enemy)
            {
                enemyOnTile = true;
                enemyCount++;
                Debug.Log("Selected Enemy Unit " + unitOnTile.UnitName + " is on tile " + unitOnTile.Position);
            }

        }

        
        TileControl tileControl = new TileControl();

        // Mark tile as ally or enemy based on units on it
        if (allyOnTile && enemyOnTile)
        {
            tileControl = TileControl.Contested;
        }
        else if (allyOnTile)
        {
            tileControl = TileControl.Ally;
        }
        else if (enemyOnTile)
        {
            tileControl = TileControl.Enemy;
        }
        else
        {
            tileControl = TileControl.Empty;
        }

        // Here tilecontrol is the key, and unit list is the value
        Dictionary<TileControl,List<Unit>> occupyingUnits = new Dictionary<TileControl,List<Unit>>(); 
        occupyingUnits.Add(tileControl, unitListOnTile);

        return occupyingUnits;
    }

    // Calculates who goes next in the turn
    // Note that if both sides have zero initiative, attacker gains first turn
    public ArmyManager.Army TurnCalculator(ArmyManager.Army armyOne, ArmyManager.Army armyTwo, int turn)  // ADD MULTIARMY CALC
    {
        int iPDiff = InitiativePointManager(armyOne, armyTwo, turn);

        if (iPDiff > 0) // Turn will be armyOne's, give armyTwo +IP for next turn
        {
            armyTwo.IP++;
            return armyOne;
        } else if (iPDiff < 0) // Turn will be armyTwo's, give armyOne +IP for next turn
        {
            armyOne.IP++;
            return armyTwo;
        } else { // If IP diff is zero, attacker starts (being attacker gives only 1 turn but for contesting IP it is worth 2)
            if(armyOne.isAttacker) {return armyOne;} else {return armyTwo;}
        }

    }

    // Gives and takes initiative points (IP) for all armies based on various in-game occurences
    // (ArmyOneIP, ArmyTwoIP, IPdiff)
    // 1 initiative points gives one turn
    public int InitiativePointManager(ArmyManager.Army armyOne, ArmyManager.Army armyTwo, int turn)
    {
        int speedIPOne = 0;
        int speedIPTwo = 0;

        // Attacker get 1 IP at start, only one can be attacker but player gets priority check
        if (turn == 0)
        {
            armyOne.IP = 0;
            armyTwo.IP = 0;
            armyOne.ExtraIP = 0;
            armyTwo.ExtraIP = 0;

            if (armyOne.isAttacker)
            {
                armyOne.IP++;
            } else if (armyTwo.isAttacker)
            {
                armyTwo.IP++;
            }

            // Add speed IP based on army mobility and a speed factor
            float speedFactor = 100f;
            speedIPOne = RoundDown((float)armyOne.ArmyMobility/speedFactor);
            speedIPTwo = RoundDown((float)armyTwo.ArmyMobility/speedFactor);

            
            // If ambusher, multiply speed IPs and nullify enemy IPs
            int ambusherMulti = 2;
            int ambushedMulti = 0;
            if (armyOne.isAmbusher)
            {
                speedIPOne = speedIPOne*ambusherMulti; // Can add probabilistic interp (instead of rounding up/down) 
                speedIPTwo = speedIPTwo*ambushedMulti;
            } else if (armyTwo.isAmbusher)
            {
                speedIPTwo = speedIPTwo*ambusherMulti;
                speedIPOne = speedIPOne*ambushedMulti;
            }

            int speedIPdiff = speedIPOne - speedIPTwo;
            if (speedIPdiff >= 0)
            {
                armyOne.IP = armyOne.IP + speedIPdiff;
            } else {
                armyTwo.IP = armyTwo.IP + speedIPdiff;
            }


        } else if (turn != 0)
        {  
            float extraturnFactor = 300f;

            // Extra IP mid-battle based on speed differences + an extra turn factor
            // IP is stored in decimal here, if above 1, add IP and subtract 1 from the stored value.
            armyOne.ExtraIP = armyOne.ExtraIP + (float)armyOne.ArmyMobility/extraturnFactor;
            armyTwo.ExtraIP = armyTwo.ExtraIP + (float)armyTwo.ArmyMobility/extraturnFactor;

            float ExtraIPdiff = armyOne.ExtraIP - armyTwo.ExtraIP;
            if (ExtraIPdiff >= 0)
            {
                armyTwo.ExtraIP = 0;
                armyOne.ExtraIP = ExtraIPdiff;
                // If greater than 1, add extra turn (IP) and subtract from remaining
                if (armyOne.ExtraIP > 1)
                {
                    armyOne.ExtraIP = armyOne.ExtraIP - 1;
                    armyOne.IP++;
                }

            } else {
                armyOne.ExtraIP = 0;
                armyTwo.ExtraIP = -ExtraIPdiff;
                // If greater than 1, add extra turn (IP) and subtract from remaining
                if (armyTwo.ExtraIP > 1)
                {
                    armyTwo.ExtraIP = armyTwo.ExtraIP - 1;
                    armyTwo.IP++;
                }
            }

        }

        

        // Talent based starting IPs
        //CODE HERE

        // Add IPs based on ability
        
        // Can add storage of mobility diff creating extra IP for mid-game additional moves later

        // Get IP diff to determine turn sequence
        int iPdiff = armyOne.IP - armyTwo.IP;
        
        Debug.Log("IP diff = " + iPdiff.ToString());
        
        return (iPdiff);
        
    }

    //if (Piece.Faction == Affiliation.Ally)
    //{
    //    UpdateUnit(ArmyManager.AllyUnitList, ArmyManager.AllyUnitDict, Piece.UnitId, Piece);
    //} else if (Piece.Faction == Affiliation.Enemy)
    //{
    //    UpdateUnit(ArmyManager.EnemyUnitList, ArmyManager.EnemyUnitDict, Piece.UnitId, Piece);
    //}

    private void SetAttacker(ArmyManager.Army attackingArmy)
    {
        foreach (ArmyManager.Army army in armyList)
        {
            army.isAttacker = false;
        }
        attackingArmy.isAttacker = true;
    }

    // Committing a move (adding to move and turn history, cause refresh of map)
    public void CommitMove(Move move, bool isMoveDone = false)
    {
        //preMoveTotal = 0; // Reset pre/post moves
        //postMoveTotal = 0;


        CreateText("turnText", "turn number:...", 20f, 5f, new Vector3(-10f,-10f,-0.25f), Color.black);
        // Add to move and turn/map history
        MapStateRecord(move);


        // Move the piece
        if (!isMoveDone) // Do the move if it hasn't been done (before), but add tag to not repeat this function
        {
            PieceMovement PMove = GetComponent<PieceMovement>();
            PMove.MovePieceTo(move.Piece, new Vector3Int (move.TargetSquare.x,move.TargetSquare.y,0), true); //2ND PARAMETER MIGHT BE WRONG
        }
        
        // Refresh/recalculate the map state
        //SOMETHING TO DO WITH UNIT IS ON TILE AND REFRESHIGN
        


        turn++;
        return;
    }

    // Intermezzo move, queued move either after a turn, or before a future turn. These can be automatic or triggered by an event (QueueMove() ).
    public void IntermezzoMove(Move inbetweenmove, bool isPreMove)
    {
        // If not pre-move, it is post-move
        if (isPreMove)
        {
            preMoveTotal[turn]++;
        } else
        {
            postMoveTotal[turn]++;
        }


        // Create turn string ()


        // Add to move and turn/map history
        MapStateRecord(inbetweenmove);


        // Refresh/recalculate the map state


        return;
    }

    // Queues a turn until a given turn later down the line. Adds to relevant pre/post move counts and
    // adds the move into a queue (which is used later).
    public void QueueMove(Move queuedMove, int turnsLater)
    {    
        // See if move is instant, premove, postmove (or none if nothing) 
        foreach (string moveTag in queuedMove.MoveTags)
        {
            if (moveTag.Contains("pre"))
            {
                if (turnsLater == 0) {turnsLater++;}

                while (turn+turnsLater > preMoveTotal.Count-1) {preMoveTotal.Add(0);} // Add into premove list until turn number reached.

                preMoveTotal[turn+turnsLater]++;
                moveQueue[turn+turnsLater].Add(queuedMove);
                
                break;
            } else if (moveTag.Contains("post"))
            {
                while (turn+turnsLater > postMoveTotal.Count-1) {postMoveTotal.Add(0);} // Add into postmove list until turn number reached.

                postMoveTotal[turn+turnsLater]++;
                moveQueue[turn+turnsLater].Add(queuedMove);
                break;
            } else if (moveTag.Contains("instant"))
            {
                instantMoveCount++;
                //INSTANT TURN
                break;
            }

            UpdateTurnSequence(turn+turnsLater);

        }
        




        return;
    }

    // Updates the turn sequence string for a given turn
    public void UpdateTurnSequence(int turnNumber)
    {
        // Guard function, you cannot change past turn (since instant moves already done...)
        if (turnNumber < turn) return;

        while (turnNumber > preMoveTotal.Count-1) {preMoveTotal.Add(0);} // Add into premove list until turn number reached.

        while (turnNumber > postMoveTotal.Count-1) {postMoveTotal.Add(0);} // Add into postmove list until turn number reached.


        string totalTurnInstance = "";
        string tempString = "";
        for (int  i = 0; i < preMoveTotal[turnNumber]; i++)
        {
            tempString = tempString + "-";
        }
        totalTurnInstance = tempString; 

        totalTurnInstance = totalTurnInstance + turn.ToString();

        tempString = "";
        for (int  i = 0; i < postMoveTotal[turnNumber]; i++)
        {
            tempString = tempString + "+";
        }
        totalTurnInstance = totalTurnInstance + tempString;

        if (turnNumber > allRealTurns.Count-1)
        {
            UpdateTurnSequence(allRealTurns.Count); // Extend Turn sequence for all unsequenced turns until this turn number. Note, no formal recursion break.
        } else {
            allRealTurns[turnNumber] = totalTurnInstance;
        }
        return;
    }

    public void TurnStart()
    {
        turn++;

        // UPDATE TURNSEQUENCE (IF NOT DONE SO)
        UpdateTurnSequence(turn);

        // EXECUTE PREMOVES

        // PLAYER/AI CAN MOVE

        // EXECUTE POSTMOVES

        int i=1;
        // TODO: Get queued moves and make a turn sequence using the pre and post-moves
    }

    public void ExecuteMove(Move move)
    {
        int i=1;

    }



    public void MapUpdate()
    {
        int i=1;
        //MapStateRecord();
    }

    // Updates the current internal turn state
    public void GetCurrentRealTurn()
    {
        // Turn string
        //realTurnSequence = turn.ToString();

        // The first pre-move committed will be the first pre-move action taken,
        // the first post-move committed is the first post-move action (first come, first serve)
        // Queueing a (immediate) pre-move after the last move, but before the next move means the pre-move is committed for NEXT turn.
        // Queueing a (immediate) post-move after the last move, but before the next move means the post-move is committed for THIS turn.
        // Instant actions "!", are done immediately after whatever preceded it.

        string totalTurnInstance = "";
        string tempString = "";
        for (int  i = 0; i < preMoveTotal[turn]; i++)
        {
            tempString = tempString + "-";
        }
        totalTurnInstance = tempString; 

        totalTurnInstance = totalTurnInstance + turn.ToString();

        tempString = "";
        for (int  i = 0; i < postMoveTotal[turn]; i++)
        {
            tempString = tempString + "+";
        }
        totalTurnInstance = totalTurnInstance + tempString;


    }

    // Stores the current internal turn
    public void StoreCurrentTurn()
    {
        int i=1;
        return;
    }

    TMP_Text CreateText(string textName, string textContent, float textWidth, float textHeight, Vector3 textPosition, Color outlineColor) // TODO: MAKE A CODE THAT CAN PRINT A TEXT IN THE GAME (IDEALLY IN THE UI)
    {
        // TODO: GET UI (CANVAS) GAMEOBJECT AND SET TEXT AS ITS PARENT


        // The x coord tile
        GameObject textObject = new GameObject(textName); //Text
        //textObject.transform.parent = transform;

        TMP_Text textMesh = textObject.AddComponent<TextMeshPro>();
        //textMesh.font = font;
        //textMesh.fontSize = fontSize;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.color = Color.black;
        textMesh.alignment = TextAlignmentOptions.Center;
        
        textMesh.text = textContent;

        // Size and Width of text
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
        // Positioning and alignment
        textObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        textObject.transform.position = textPosition;
        // Rendering layer
        textMesh.GetComponent<Renderer>().sortingLayerName = "UI";

        return textMesh;

    }


}
