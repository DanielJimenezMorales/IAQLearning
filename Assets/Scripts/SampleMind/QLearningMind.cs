using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class QLearningMind : AbstractPathMind
    {
        #region Variables generales
        private float[,] qTable = new float[4, 112]; //y = estados, x = acciones
        private float alpha = 0.5f;
        private float gamma = 0.5f;
        #endregion

        #region Variables explorar
        private const int N_EPI_MAX = 10; // número de episodios como máximo. (Número de veces que se repite el algoritmo partiendo de una casilla
        //Aleatoria vacía)
        private const int N_ITER_MAX = 100; // número de iteraciones por cada episodio como máximo.
        #endregion

        #region Variables explotar
        #endregion



        public override void Repath()
        {

        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            // EstructuraDatosCamino será una estructura de datos
            // (pila, cola, árbol, tabla....) conveniente para este problema.
            // Debe contener las acciones que lleven desde el inicio hasta la meta.
            /*if EstructuraDatosCamino.IsVacia
            {
                qTable = Explorar()
            EstructuraDatosCamino = Explotar(TablaQ, celda_inicial)
            val = EstructuraDatos.Leer()
            }
            else
            {
                val = EstructuraDatos.Leer()
            }
            if (val == 0) return Locomotion.MoveDirection.Up;
            if (val == 1) return Locomotion.MoveDirection.Down;
            if (val == 2) return Locomotion.MoveDirection.Left;*/
            return Locomotion.MoveDirection.Right;
        }

        private float Explorar(CellInfo initialCell, BoardInfo board, CellInfo[] goals)
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
                    currentAction = GetRandomAction();
                    nextCell = GetNeighbourCell(currentCell, currentAction);//Solucionar
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
            return 0f;
        }

        private CellInfo GetNeighbourCell(CellInfo currentCell, int action)
        {
            int newRow = -1;
            int newColumn = -1;

            if(action == 0)
            {

            }
        }

        private CellInfo GetInitialRandomCell(BoardInfo board)
        {
            int randomRow = Random.Range(0, board.NumRows);
            int randomColumn = Random.Range(0, board.NumColumns);
            return board.CellInfos[randomRow, randomColumn];
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
            return (cell.RowId * board.NumRows) + cell.ColumnId; //?
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
    }
}