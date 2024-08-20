using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class ToolBar
    {
        Graph _graph;
        enum ToolState
        {
            None,
            Endpoint,
            Bridge,
            Wall,
            Portal,
            Player,
            Computer,
            Automatic
        }
        ToolState _state;
        ToolState State {
            set { _state = value; }
            get { return _state; }
        }

        public ToolBar()
        {
            _state = ToolState.None;
        }

        public void Init(Graph graph)
        {
            _graph = graph;
        }

        public void Update()
        {
            
        }

        public void Draw()
        {
            DrawOutlines();
        }

        private void DrawOutlines()
        {
            for (int i = 0; i < 9; i++)
            {
                Flow.Sd.DrawLine(Flow.CellDim * new Vector2(i + 1, Flow.GraphDim + 2), Flow.CellDim * new Vector2(i + 1, Flow.GraphDim + 3), Color.White);
            }
            for (int i = 0; i < 2; i++)
            {
                Flow.Sd.DrawLine(Flow.CellDim * new Vector2(1, Flow.GraphDim + 2 + i), Flow.CellDim * new Vector2(2, Flow.GraphDim + 2 + i), Color.White);
                Flow.Sd.DrawLine(Flow.CellDim * new Vector2(3, Flow.GraphDim + 2 + i), Flow.CellDim * new Vector2(7, Flow.GraphDim + 2 + i), Color.White);
                Flow.Sd.DrawLine(Flow.CellDim * new Vector2(8, Flow.GraphDim + 2 + i), Flow.CellDim * new Vector2(9, Flow.GraphDim + 2 + i), Color.White);
            }
        }
    }
}
