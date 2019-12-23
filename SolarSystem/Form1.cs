using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL;
using SharpGL.WinForms;
using SharpGL.SceneGraph;
using SharpGL.Enumerations;
using SharpGL.SceneGraph.Assets;
using System.Diagnostics;

namespace SolarSystem
{
    public partial class Form1 : Form
    {
        OpenGL gl;
        float planetDt = 0;//Количество пройденных дней 
        double dt = 10.0d;
        float day;
        const float hour = 0.04167f;//Движение в час 1/24
        int view = 25;//Угол обзора
        int positionX = 7;
        int positionY = 3;
        int positionZ = 0;
        bool rotate_cam = false;//Вращать камеру по кругу?   
        bool pause = true;//Пауза                          
        bool isAster = true;

        float rotate_cam_Angle = 0.0f;

        const float Planet = 0.5910f;//360°/Период вращения - 25ᵈ 9ʰ 7ᵐ((25*24+9)*60+7/60) часа


        IntPtr QPlanet, QAsteroid;
        Texture TPlanet, TAsteroid, Tstars;


        public Form1()
        {
            InitializeComponent();

            #region Инициализация для звзед
            Random k = new Random();
            for (int i = 0; i < n; i++)
            {
                x[i] = k.Next(-100, 100);
                y[i] = k.Next(-100, 100);
                z[i] = k.Next(-100, 100);
                colorGrey[i] = k.NextDouble() / 2 + 0.5;
            }
            #endregion
            
            openGLControl1.OpenGLDraw += openGLControl1_OpenGLDraw;

            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.WindowState = FormWindowState.Normal;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Random k = new Random();
            for (int i = 0; i < n; i++)
                colorGrey[i] = k.NextDouble() + 0.4f;
        }

        static int n = 10000;
        int[] x = new int[n];
        int[] y = new int[n];
        int[] z = new int[n];
        double[] colorGrey = new double[n];
        float[][] constellation_LittleBear =
        {
            new float[] {0f, 0f, 0f },
            new float[] {1f, -0.7f, 0f},
            new float[] {2f, -1f, 0f},
            new float[] {3f, -0.8f, 0f},
            new float[] {4f, -0.5f, 0f},
            new float[] {4.3f, -1f, 0f},
            new float[] {3.4f, -1.25f, 0f},
            new float[] {3f, -0.8f, 0f}
        };
        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            textBox3.Text = planetMass.ToString();
            textBox4.Text = asteroidMass.ToString();
            textBox1.Text = planetRadius.ToString();

            velosityX.Text = p.vx.ToString();
            velosityY.Text = p.vy.ToString();
            velosityZ.Text = p.vz.ToString();

            gl = openGLControl1.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60.0f, openGLControl1.Width / openGLControl1.Height, 1.0f, 100.0f);

            initTextures();//Создаем текстуры
            initPlanet();//Создаем модели планет
        }

        const double gravity = 9.8;// гравитационная постоянная
        const int fmax = int.MaxValue - 1000; // максимальное значение силы
        struct Particle
        {
            public double x, y, z, vx, vy, vz;
        };
        struct Force
        {
            public double x, y, z;
        };
        Particle p;
        Force f;

        double planetMass = 5.9730000;// earth
        double asteroidMass = 0.01;// zerera
        double planetRadius = 6.378;
        double distance = 70;

        private void Отобразить_Click(object sender, EventArgs e)
        {
            pause = !pause;
            isAster = !isAster;
            planetMass = Convert.ToDouble(textBox3.Text);
            asteroidMass = Convert.ToDouble(textBox4.Text);
            planetRadius = Convert.ToDouble(textBox1.Text);

            p.vx = Convert.ToDouble(velosityX.Text);
            p.vy = Convert.ToDouble(velosityY.Text);
            p.vz = Convert.ToDouble(velosityZ.Text);

            //startVelocity = Convert.ToDouble(textBox5.Text);
            //distanceOb = Convert.ToDouble(textBox2.Text);
            //distanceOb = Convert.ToDouble(textBox2.Text);
        }

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            #region Вращение камеры вокруг солнца
            if (rotate_cam)
            {
                float TimeDelta = 0.15f;
                gl.Rotate(rotate_cam_Angle * TimeDelta, 0, 1, 0);//Камеры

                rotate_cam_Angle += 1;//Добовляем 1

            }
            else
                rotate_cam_Angle = 0;//Если вращение отключеноь,то вовзращаемся в начальную позицию
            #endregion
            #region Планета
            TPlanet.Bind(gl);//устанавливаем текстуру солнца

            gl.PushMatrix(); //отсутствие вращения
            gl.Rotate(planetDt * Planet, 0, 1, 0);//вращение солнца
            gl.Rotate(-5000, 0, 0);
            gl.Color(1f, 1f, 1f);//Цвет которым оно светит
            gl.Sphere(QPlanet, 3f, 40, 40);//Прорисовываем квадрик
            gl.PopMatrix();
            #endregion
            #region Звезды
            Tstars.Bind(gl);//Текестура звезд

            gl.PointSize(1.5f);
            gl.Enable(OpenGL.GL_POINT_SMOOTH);
            gl.Begin(BeginMode.Points);
            for (int i = 0; i < n; i++)
            {
                //gl.Color(5f, 5f, 5f);
                gl.Color(colorGrey[i], colorGrey[i], colorGrey[i]);
                //gl.Color(cl_Grey, cl_Grey, cl_Grey);
                gl.Vertex(x[i], y[i], z[i]);
            }
            gl.End();
            gl.Disable(OpenGL.GL_POINT_SMOOTH);
            #endregion


            initlighting();//Источник света
            gl.LoadIdentity();

            gl.LookAt(0, view, 15, 0, 0, 0, 0, 1, 0);
            gl.Rotate(150, 1, 0, 0);
            #region Астероид

            //Вращение Астероида
            gl.PushMatrix();
            gl.PushMatrix();
            if (pause)
            {
                //gl.Translate(positionX, positionY, positionZ);
                gl.PushMatrix();
                gl.PushMatrix();
                //gl.Rotate(Asteroid * day, 0, 1, 0);//Вокруг планеты
                gl.Translate(positionX, positionY, positionZ);
                gl.Color(0.8235f, 0.7373f, 0.6824f, 1f);
                TAsteroid.Bind(gl);

                gl.Rotate(-5000, 0, 0);//Вокруг себя
                gl.Sphere(QAsteroid, 0.5f, 40, 50);
                gl.PopMatrix();


                gl.PopMatrix();
               
            }
            else
            {
                int koef = 0;
                const float moonRP = 7.4194f;
                const float hour = 0.04167f;
                day += hour * 6;
                if (koef == 0)// по элипсу?
                {
                   
                    gl.PushMatrix();
                    gl.Rotate(moonRP * day, 0, 1, 1);//Вокруг солнца
                    gl.Translate(positionX, positionY, positionZ);
                    gl.Color(0.8235f, 0.7373f, 0.6824f, 1f);
                    
                    TAsteroid.Bind(gl);
                    gl.Sphere(QAsteroid, 0.12f, 25, 25);
                    gl.PopMatrix();
                }
                else
                {
                    p.x = positionX * distance;
                    p.y = positionY * distance;
                    p.z = positionZ * distance;
                    CalcForces1();
                    MoveParticlesAndFreeForces();
                    //gl.Translate(p.x,p.y, p.z);

                    double normalVector = Math.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);

                    double astPosX = p.x / normalVector * 10;
                    double astPosY = p.y / normalVector * 10;
                    double astPosZ = p.z / normalVector * 7;

                    label26.Text = "p.x = " + p.x.ToString();
                    label27.Text = "p.vx = " + p.vx.ToString();
                    label28.Text = "f.x = " + f.x.ToString();

                    label29.Text = "p.x = " + p.x.ToString();
                    label30.Text = "p.y = " + p.y.ToString();
                    label31.Text = "p.z = " + p.z.ToString();

                    label32.Text = "distance = " + normalVector;

                    gl.Translate(astPosX, astPosY, astPosZ);
                }
            }
            gl.Color(0.8235f, 0.7373f, 0.6824f, 1f);
            TAsteroid.Bind(gl);

            gl.Rotate(-5000, 0, 0);//Вокруг себя
            gl.Sphere(QAsteroid, 0.5f, 40, 50);
            gl.PopMatrix();

            gl.PopMatrix();
            #endregion
            void CalcForces1()
            {
                double r = (1 / (p.x * p.x + p.y * p.y + p.z * p.z));
                double r_1 = Math.Sqrt(r);
                double fabs = gravity * asteroidMass * planetMass * r;

                if (fabs < fmax)
                {
                    f.x = fabs * p.x * r_1;
                    f.y = fabs * p.y * r_1;
                    f.z = fabs * p.z * r_1;
                }
                else
                {
                    fabs = fmax;
                }
            }
            void MoveParticlesAndFreeForces()
            {
                double dvx = f.x * dt / asteroidMass;
                double dvy = f.y * dt / asteroidMass;
                double dvz = f.z * dt / asteroidMass;

                p.x += (p.vx + dvx / 2) * dt;
                p.y += (p.vy + dvy / 2) * dt;
                p.z += (p.vy + dvz / 2) * dt;

                p.vx += dvx;
                p.vy += dvy;
                p.vy += dvz;
            }
            gl.Disable(OpenGL.GL_LIGHT0);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            gl.Disable(OpenGL.GL_TEXTURE_2D);


            if (!pause)
            { 
                try
                {
                    planetDt += hour * 6;
                }
                catch (Exception)
                {
                    planetDt = 0;
                }
            }
        }

        private void Увеличить_Click(object sender, EventArgs e)
        {
            dt += 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dt -= 1;
        }  

        private void button2_Click(object sender, EventArgs e)
        {
            rotate_cam = !rotate_cam;
        }

        private void initTextures()
        {
            Tstars = new Texture();
            Tstars.Create(gl, new Bitmap("texture\\starsColor.jpg"));
            Bitmap bmp;
            #region Текстура Планеты
            TPlanet = new Texture();
            bmp = new Bitmap(@"texture/planet.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            TPlanet.Create(gl, bmp);
            #endregion

            #region Текстура Астероида
            TAsteroid = new Texture();
            bmp = new Bitmap(@"texture/moonmap.jpg");
            //bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            TAsteroid.Create(gl, bmp);
            #endregion

        }//Инициализия текстур

        private void openGLControl1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            view++;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            view--;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (isAster)
                positionX++;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (isAster)
                positionX--;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (isAster)
                positionY++;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (isAster)
                positionY--;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (isAster)
                positionZ++;
        }


        private void button10_Click(object sender, EventArgs e)
        {
            if (isAster)
                positionZ--;
        }

        private void initlighting()
        {
            float[] materialAmbient = { 0.05f, 0.05f, 0.05f, 1.0f };
            float[] materialDiffuse = { 1f, 1f, 1f, 1.0f };
            float[] materialShininess = { 10.0f };
            float[] lightPosition = { 0f, 0f, 0f, 1.0f };
            float[] lightAmbient = { 0.75f, 0.75f, 0.75f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, lightAmbient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, lightPosition);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, materialShininess);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, materialDiffuse);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, materialAmbient);

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
        }//Инициализия освещения
        private void initPlanet()
        {
            QPlanet = iniQuadric(OpenGL.GLU_INSIDE);//Инициализируем планету с нормалями повернутыми внут
            QAsteroid = iniQuadric(OpenGL.GLU_OUTSIDE);
        }//Инициализия квадриков планет
        private IntPtr iniQuadric(uint glu_mode_orientation = OpenGL.GLU_OUTSIDE)
        {
            IntPtr planet = gl.NewQuadric();
            gl.QuadricTexture(planet, (int)OpenGL.GL_TRUE);//Активируем текстуру на квадрике
            gl.Enable(OpenGL.GL_RESCALE_NORMAL_EXT);//При однородном масштабирование применяем что нормализовать нормали
            gl.QuadricOrientation(planet, (int)glu_mode_orientation);//Указываем положение нормалей
            gl.QuadricDrawStyle(planet, OpenGL.GLU_FILL);//Тип прорисовки
            gl.QuadricNormals(planet, OpenGL.GL_SMOOTH);//Сглаживание
            return planet;//Возвращаем квадрик
        }//Возвращаем квадрик с установленными настройками

    }
}