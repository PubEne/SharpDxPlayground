using SharpDX;
using SharpDX.D3DCompiler;
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
        private Viewport viewport;


        private D3D11.VertexShader vertexShader;
        private D3D11.PixelShader pixelShader;
        private ShaderSignature inputSignature;
        private D3D11.InputLayout inputLayout;

        private D3D11.InputElement[] inputElements = new D3D11.InputElement[]
        {
            new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, D3D11.InputClassification.PerVertexData, 0),
            new D3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, D3D11.InputClassification.PerVertexData, 0)
        };

        private Vector3[] vertices = new Vector3[] { new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.0f, -0.5f, 0.0f) };
        private D3D11.Buffer triVertexBuffer;


        public Game()
        {
            renderForm = new RenderForm("hello");
            renderForm.ClientSize = new Size(Width, Height);
            renderForm.AllowUserResizing = false;

            InitializeDeviceResources();
            InitializeShaders();
            InitTriangle();
        }
        
        #region Direct3DInit
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
                IsWindowed = true
        };

            //creating device and swapchain
            // on GPU, not using special flags, SwapChain, 
            D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDescription, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;


            //creating viewport
            viewport = new Viewport(0, 0, Width, Height);
            d3dDeviceContext.Rasterizer.SetViewport(viewport);

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

        //compiling shaders...
        private void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new D3D11.VertexShader(d3dDevice, vertexShaderByteCode);
            }
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug))
            {
                pixelShader = new D3D11.PixelShader(d3dDevice, pixelShaderByteCode);
            }


            ///seting up device context
            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);

            //seting up a topology. that defines how the vertices should be drawn.
            //https://docs.microsoft.com/en-us/windows/desktop/direct3d11/images/d3d10-primitive-topologies.png
            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            //creating layout
            inputLayout = new D3D11.InputLayout(d3dDevice, inputSignature, inputElements);

            d3dDeviceContext.InputAssembler.InputLayout=inputLayout;
        }

        //initialize trinagle
        private void InitTriangle()
        {
            triVertexBuffer = D3D11.Buffer.Create<Vector3>(d3dDevice, D3D11.BindFlags.VertexBuffer, vertices);
        }


        private void Draw()
        {
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0.2f, 0.4f, 0.6f, 1.0f));

            d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(triVertexBuffer, Utilities.SizeOf<Vector3>(), 0));
            d3dDeviceContext.Draw(vertices.Count(), 0);

            //swap buffers
            swapChain.Present(1, PresentFlags.None);
        }

        // dispose unused objects
        public void Dispose()
        {
            inputLayout.Dispose();
            inputSignature.Dispose();
            triVertexBuffer.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();
        }
        #endregion
    }
}