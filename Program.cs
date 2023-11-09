
using System.Diagnostics;
// using OpenTK.Graphics.GL;
// using OpenTK.Graphics.ES20;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTK.Graphics.OpenGL4;


namespace OpenToolKit
{
    public class Program
    {
        public static readonly float[] triangleVTX =
        [
            0.0f,0.5f,0.0f,
            0.5f,-0.5f,0.0f,
            -0.5f,-0.5f,0.0f
        ];


        public static void Main(string[] args)
        {
            using (Game game = new Game(1280, 720, "OpenToolKit"))
            {
                game.Run();
            }
        }
    }


    public class Game : GameWindow
    {
        private int VertexBufferHandle = default(int);
        private int VertexArrayObject = default(int);
        private Shader? shader = null;

        public Game(int width, int height, string title)
         : base(new GameWindowSettings()
         {
             UpdateFrequency = 60,
             //RenderFrequency = 60 //Obsulete
         }, new NativeWindowSettings()
         {
             Size = new Vector2i(width, height),
             Title = title,
             WindowBorder = WindowBorder.Fixed
         })
        {

        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(.1f, .3f, .4f, 1f);

            VertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, Program.triangleVTX.Length * sizeof(float), Program.triangleVTX, BufferUsageHint.StaticDraw);

            shader = new Shader("shader.vert", "shader.frag");


            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, Program.triangleVTX.Length * sizeof(float), Program.triangleVTX, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            shader.UseShaders();
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); //default binding estate
            GL.DeleteBuffer(VertexBufferHandle); //delete the buffer

            shader!.Dispose();
            GL.UseProgram(0);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            //!first logic then render

            base.OnUpdateFrame(args);

            //60 FPS
            Console.WriteLine(1 / this.UpdateTime != 0 ? 1 / this.UpdateTime : 0);

            if (KeyboardState.IsKeyDown(Keys.Escape)) //ESC
            {
                this.Close();
            }

            this.CenterWindow((1280, 720)); //centers and sets resolution
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader!.UseShaders();
            GL.UseProgram(1); 
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            this.SwapBuffers();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }

    public class Shader : IDisposable
    {
        int ShaderHandle = default(int);
        private bool disposed = default(bool);
        public Shader(string vertexShaderPath, string fragmentShaderPath)
        {
            int VertexShader;
            int FragmentShader;

            string VertexShaderSourceCode = File.ReadAllText(vertexShaderPath);
            string FragmentShaderSourceCode = File.ReadAllText(fragmentShaderPath);

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSourceCode);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSourceCode);



            GL.CompileShader(VertexShader);
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int vertexSuccess);
            if (vertexSuccess == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);
            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int fragmentSuccess);
            if (fragmentSuccess == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine(infoLog);
            }

            ShaderHandle = GL.CreateProgram();
            GL.AttachShader(ShaderHandle, VertexShader);
            GL.AttachShader(ShaderHandle, FragmentShader);
            GL.LinkProgram(ShaderHandle);

            GL.GetProgram(ShaderHandle, GetProgramParameterName.LinkStatus, out int shaderSuccess);
            if (shaderSuccess == 0)
            {
                string infoLog = GL.GetProgramInfoLog(shaderSuccess);
                Console.WriteLine(infoLog);
            }

            GL.DetachShader(ShaderHandle, VertexShader);
            GL.DetachShader(ShaderHandle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }



        public void UseShaders()
        {
            GL.UseProgram(ShaderHandle);
        }



        protected virtual void LetsDispose()
        {
            if (!disposed)
            {
                GL.DeleteProgram(ShaderHandle);
                disposed = true;
            }
        }

        ~Shader()
        {
            if (!disposed)
            {
                Console.WriteLine("GPU Resource leak!");
            }
        }


        public void Dispose()
        {
            LetsDispose();
            GC.SuppressFinalize(this);
        }
    }
}