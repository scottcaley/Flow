using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class Graph
    {
        ToolBar _toolBar;
        public Graph()
        {

        }
        public void Init(ToolBar toolBar)
        {
            _toolBar = toolBar;
        }

        public void Update()
        {

        }

        public void Draw()
        {
            for (int i = 0; i <= Flow.GraphDim; i++)
            {
                Flow.Sd.DrawLine((float)Flow.CellDim * new Vector2(1, i + 1), (float)Flow.CellDim * new Vector2(Flow.GraphDim + 1, i + 1), Color.White);
                Flow.Sd.DrawLine((float)Flow.CellDim * new Vector2(i + 1, 1), (float)Flow.CellDim * new Vector2(i + 1, Flow.GraphDim + 1), Color.White);
            }
        }
    }
}
