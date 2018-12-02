using SharpDX.Windows;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTest
{
    public class Game : IDisposable
    {
        private RenderForm renderForm;

        private const int Width = 1280;
        private const int Height = 720;


        public Game()
        {
            renderForm = new RenderForm("hello");
            renderForm.ClientSize = new Size(Width, Height);
            renderForm.AllowUserResizing = false;
        }


        //render game loop
        public void Run() {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        //callled every frame
        void RenderCallback()
        {

        }


        // dispose unused objects
        public void Dispose() {
            renderForm.Dispose();
        }

    }
}