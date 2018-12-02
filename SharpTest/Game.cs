using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;

namespace SharpTest
{
    public class Game : IDisposable
    {
        private RenderForm renderForm;

        private const int Width = 1280;
        private const int Height = 720;

        private D3D11.Device d3dDevice;
        private D3D11.DeviceContext d3dDeviceContext;
        private SwapChain swapChain;
        private D3D11.RenderTargetView renderTargetView;



        public Game()
        {
            renderForm = new RenderForm("hello");
            renderForm.ClientSize = new Size(Width, Height);
            renderForm.AllowUserResizing = false;

            InitializeDeviceResources();
        }


        //render game loop
        public void Run() {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        //callled every frame
        private void RenderCallback()
        {
            Draw();
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/bb173064(v=vs.85).aspx
        private void InitializeDeviceResources() {
            ModeDescription modeDescription = new ModeDescription(Width, Height, new Rational(60,1), Format.R8G8B8A8_UNorm);
            SwapChainDescription swapChainDescription = new SwapChainDescription()
            {
                //back buffer  description
                ModeDescription = modeDescription,
                //no multisampling
                SampleDescription = new SampleDescription(1, 0),
                //how cpu accces back buffer
                Usage = Usage.RenderTargetOutput,
                //buffer count
                BufferCount = 1,
                //in which window
                OutputHandle = renderForm.Handle,
                //windowed or full
                IsWindowed=true
            };

            //creating device and swapchain
            // on GPU, not using special flags, SwapChain, 
            D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDescription, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;

            //https://stackoverflow.com/questions/10057334/when-should-i-use-the-using-statement
            using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0))
            {
                renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);
            }
            //try
            //{
            //    D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0);
            //    renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);
            //}
            //finally
            //{
            //    renderTargetView.Dispose();
            //}
        }

        private void Draw()
        {
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(32, 103, 178, 125));
            swapChain.Present(1, PresentFlags.None);
        }


        // dispose unused objects
        public void Dispose()
        {
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();
        }


    }
}