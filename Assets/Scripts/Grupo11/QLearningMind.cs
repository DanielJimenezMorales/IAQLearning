using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class QLearningMind : AbstractPathMind
    {
        #region Variables generales
        private Queue<int> currentPlan = new Queue<int>();
        private float[,] qTable = new float[4, 112]; //y = estados, x = acciones
        
        private QTextFileWriter writer;
        #endregion

        #region Variables explorar
        [SerializeField] [Range(10, 1000)] [Tooltip("número de episodios como máximo. (Número de veces que se repite el algoritmo partiendo de una casilla aleatoria vacía)")]
        private int N_EPI_MAX = 100;
        [SerializeField] [Range(10, 1000)] [Tooltip("Número de iteraciones por cada episodio como máximo.")]
        private int N_ITER_MAX = 100;

        [SerializeField] [Range(0f, 1f)] [Tooltip("Ritmo de aprendizaje.")]
        private float alpha = 0.5f;
        [SerializeField] [Range(0f, 1f)] [Tooltip("Ratio de descuento.")]
        private float gamma = 0.5f;
        #endregion

        public override void Repath()
        {
            currentPlan.Clear();
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            int currentMovement = -1;

            if(currentPlan.Count == 0)
            {
                Explorar(boardInfo, goals);

                if (writer == null)
                {
                    writer = new QTextFileWriter();
                }
                writer.CreateText("/QTable.txt", QTableInText());

                currentPlan = Explotar2(currentPos, boardInfo, goals);
                currentMovement = currentPlan.Dequeue();
            }
            else
            {
                currentMovement = currentPlan.Dequeue();
            }

            if (currentMovement == 0) return Locomotion.MoveDirection.Up;
            if (currentMovement == 1) return Locomotion.MoveDirection.Down;
            if (currentMovement == 2) return Locomotion.MoveDirection.Right;
            
            return Locomotion.MoveDirection.Left;
        }

        private void Explorar(BoardInfo board, CellInfo[] goals)
        {
            CellInfo nextCell = null;
            CellInfo currentCell = null;
            int currentIterations = 0;
            int currentAction = -1;
            bool stopCondition = false;

            //Formula variables
            float currentQ = 0f;
            float reward = 0f;
            float nextQmax = 0f;
            float newQ = 0f;

            for (int i = 0; i < N_EPI_MAX; i++)
            {
                currentIterations = 0;
                nextCell = GetInitialRandomCell(board);
                stopCondition = false;
                while(!stopCondition)
                {
                    currentCell = nextCell;
                    do
                    {
                        currentAction = GetRandomAction();
                    } while (!CheckIfNeighbourIsOut(currentCell, currentAction, board));
                    nextCell = GetNeighbourCell(currentCell, currentAction, board);
                    currentQ = GetActualQ(GetCellID(currentCell, board), currentAction);
                    //reward = GetReward(currentCell, current_action, nextCell);?????????????
                    reward = GetReward(nextCell, goals[0]);
                    nextQmax = GetNextMaxQ(GetCellID(nextCell, board));
                    newQ = UpdateRule(currentQ, reward, nextQmax);
                    UpdateQTable(GetCellID(currentCell, board), currentAction, newQ);
                    currentIterations++;
                    stopCondition = CheckExploreStopCondition(currentIterations, nextCell, goals[0]);
                }
            }
        }

        private string QTableInText()
        {
            string table = System.String.Empty;
            table += "Tabla Q:\nPrecisión de datos: 3 decimales\n";
            for (int i = 0; i < 112; i++)
            {
                string row = System.String.Empty;

                for (int j = 0; j < 4; j++)
                {
                    row += ", " + qTable[j, i].ToString("F3");
                }

                table += i + "--" + row + "\n";
            }

            return table;
        }

        private bool CheckIfNeighbourIsOut(CellInfo currentCell, int action, BoardInfo board)
        {
            CellInfo[] neighbours = currentCell.WalkableNeighbours(board);

            if (action == 0) //NORTE
            {
                if(neighbours[0] == null)
                {
                    return false;
                }
            }
            else if (action == 1) //SUR
            {
                if (neighbours[2] == null)
                {
                    return false;
                }
            }
            else if (action == 2) //ESTE
            {
                if (neighbours[1] == null)
                {
                    return false;
                }
            }
            else if (action == 3) //OESTE
            {
                if (neighbours[3] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private CellInfo GetNeighbourCell(CellInfo currentCell, int action, BoardInfo board)
        {
            int newRow = currentCell.RowId;
            int newColumn = currentCell.ColumnId;

            if (action == 0) //NORTE
            {
                newRow++;
            }
            else if (action == 1) //SUR
            {
                newRow--;
            }
            else if (action == 2) //ESTE
            {
                newColumn++;
            }
            else if (action == 3) //OESTE
            {
                newColumn--;
            }

            return board.CellInfos[newColumn, newRow];
        }

        private CellInfo GetInitialRandomCell(BoardInfo board)
        {
            int randomRow = -1;
            int randomColumn = -1;

            do
            {
                randomRow = Random.Range(0, board.NumRows);
                randomColumn = Random.Range(0, board.NumColumns);
            } while (!board.CellInfos[randomColumn, randomRow].Walkable);

            return board.CellInfos[randomColumn, randomRow];
        }

        private int GetRandomAction()
        {
            /*ACCIONES:
             * NORTE = 0,
             * SUR = 1,
             * ESTE = 2,
             * OESTE = 3*/

            return Random.Range(0, 4);
        }

        private float GetActualQ(int cellID, int actionID)
        {
            return qTable[actionID, cellID];
        }

        private int GetReward(CellInfo nextCell, CellInfo goalCell)
        {
            if (nextCell.RowId == goalCell.RowId && nextCell.ColumnId == goalCell.ColumnId)
            {
                return 100;
            }

            return 0;
        }

        private int GetCellID(CellInfo cell, BoardInfo board)
        {
            //Numero de fila * ancho de fila + numero de columna
            //Empezamos desde abajo a la izquierda y se va subiendo columnas de izquierda a derecha
            return (cell.RowId * board.NumColumns) + cell.ColumnId;
        }

        private float GetNextMaxQ(int cellID)
        {
            float max = 0f;
            for (int i = 0; i <= 3; i++)
            {
                if(qTable[i,cellID] > max)
                {
                    max = qTable[i, cellID];
                }
            }

            return max;
        }

        private float UpdateRule(float currentQ, float reward, float nextQmax) //Se usa la fórmula
        {
            return ((1 - alpha) * currentQ) + (alpha * (reward + (gamma * nextQmax)));
        }

        private void UpdateQTable(int cellID, int actionID, float newQ)
        {
            qTable[actionID, cellID] = newQ;
        }

        private bool CheckExploreStopCondition(int iterations, CellInfo nextCell, CellInfo goalCell)
        {
            if(iterations > N_ITER_MAX)
            {
                return true;
            }

            if(nextCell.RowId == goalCell.RowId && nextCell.ColumnId == goalCell.ColumnId)
            {
                return true;
            }

            return false;
        }

        private Queue<int> Explotar2(CellInfo initialPos, BoardInfo board, CellInfo[] goals)
        {
            Queue<int> path = new Queue<int>();
            CellInfo nextCell = null;
            CellInfo currentCell = null;
            nextCell = initialPos;
            bool hasFoundGoal = false;

            do
            {
                currentCell = nextCell;
                int decisionMade = GetBestAction(GetCellID(currentCell, board));

                CellInfo neighbour = GetNeighbourCell(currentCell, decisionMade, board);
                nextCell = neighbour;

                path.Enqueue(decisionMade);
                if (goals[0].RowId == neighbour.RowId && goals[0].ColumnId == neighbour.ColumnId)
                {
                    hasFoundGoal = true;
                }

            } while (!hasFoundGoal);

            return path;
        }

        private int GetBestAction(int currentCellID)
        {
            float bestQ = -Mathf.Infinity;
            int bestAction = -1;

            for (int i = 0; i < 4; i++)
            {
                if(qTable[i, currentCellID] > bestQ)
                {
                    bestQ = qTable[i, currentCellID];
                    bestAction = i;
                }
            }
            return bestAction;
        }
    }
}