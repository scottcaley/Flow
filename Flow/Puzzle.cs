using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class Puzzle {

        private Graph _graph;
        private ToolBar _toolBar;

        public Puzzle()
        {
            _graph = new Graph();
            _toolBar = new ToolBar();
            _graph.Init(_toolBar);
            _toolBar.Init(_graph);
        }

        public void Update()
        {
            _graph.Update();
            _toolBar.Update();
        }

        public void Draw()
        {
            _graph.Draw();
            _toolBar.Draw();
        }

    }
}
