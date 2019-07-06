using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Data.OleDb;
using Emgu.Util.TypeEnum;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        OleDbConnection con = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:/Users/PC/Documents/visual studio 2010/Projects/WindowsFormsApplication23/WindowsFormsApplication23/attendace.accdb");
        Image<Bgr, Byte> currentFrame, currentframe;
        Capture grabber, grab;
        HaarCascade face;
        HaarCascade eye;
        HaarCascade mouth;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, results, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        List<string> NamesPersons = new List<string>();
        int ContTrain, NumLabels, t, tem;

        // string name, names = null;
        public static string name = "";
        public static string names = "";
        public static string n = "";
        public static string ns = "";
        public static string eyename = "";
        public static string eyenames = "";

        DataTable dtbl = new DataTable();



        public Form1()
        {
            InitializeComponent();


            face = new HaarCascade("haarcascade_frontalface_default.xml");
            eye = new HaarCascade("haarcascade_eye.xml");
            mouth = new HaarCascade("haarcascade_Mouth.xml");



            try
            {
                //Loading all the previous trained face data
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels + 1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                }

            }
            catch (Exception)
            {
                //MessageBox.Show(e.ToString());
                MessageBox.Show("Nothing in binary database, please add at least a face(Simply train the prototype with the Add Face Button).", "Triained faces load", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            grabber = new Capture();
            grabber.QueryFrame();

            Application.Idle += new EventHandler(FrameGrabber);
            button1.Enabled = false; //cemra start button
        }

        public void FrameGrabber(object sender, EventArgs e)
        {


            NamePersons.Add("");

            // capture a frame form  device both face and all things on the image
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            gray = currentFrame.Convert<Gray, Byte>();
            //(TestImageBox.Image = currentFrame);
            //Result of haarCascade will be on the "MCvAvgComp"-facedetected (is it face or not )
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));//face
            MCvAvgComp[][] StoreEyes = gray.DetectHaarCascade(eye, 1.2, 2, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(1, 1));//eye
            MCvAvgComp[][] Mouthdetection = gray.DetectHaarCascade(mouth, 3, 3, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(1, 1));

            foreach (MCvAvgComp mouths in Mouthdetection[0]) // loop over all the detected MOUTHs
            {
                //currentFrame.Draw(mouths.rect, new Bgr(Color.Blue), 1);
                // draw a recangle for each mouth and put it on the display
                int mouthLableX = mouths.rect.X; // find the X top-left corner of the mouth for labling
                int mouthLableY = mouths.rect.Y; // find the Y top-left corner of the mouth for labling
                Point p = new Point(mouthLableX, mouthLableY); // create a point to attach the "MOUTH" lable to it


            }

            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                currentFrame.Draw(f.rect, new Bgr(Color.Green), 2); //Frame detect colour is 'read'
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 3000, ref termCrit);

                    name = recognizer.Recognize(result); // detected name of the face is been saved  to the 'name'-variable
                    if (name == null)
                    {
                        currentFrame.Draw(f.rect, new Bgr(Color.Green), 2); //Frame detect colour is 'read'
                    }
                    //the colour of  the face label name 
                }
                NamePersons[t - 1] = name;
                NamePersons.Add("");
                label3.Text = facesDetected[0].Length.ToString();
            }
            t = 0;
            for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
            {
                names = names + NamePersons[nnn];
            }

            label3.Text = names;
            names = "";
            NamePersons.Clear();

            NamesPersons.Add("");
            foreach (MCvAvgComp eyes in StoreEyes[0]) // more than one eye may be detected so for each eye there will be a rectangle
            {
                tem = tem + 1;
                results = currentFrame.Copy(eyes.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                currentFrame.Draw(eyes.rect, new Bgr(Color.Red), 2);
                // for each detecte eye in the image a rectangle will be drawn on the image
                int eyeLableX = eyes.rect.X; // find the X top-left corner of the eye for labeling
                int eyeLableY = eyes.rect.Y; // find the Y top-left corneer of the eye for lableing 
                Point p = new Point(eyeLableX, eyeLableY); // create a point p to attach the lable "EYE" to it
                //currentFrame.Draw(name, ref font, p, new Bgr(Color.Red)); // add a string "EYE" to the image captured from webcam just to be distingushable
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 3000, ref termCrit);
                    eyename = recognizer.Recognize(results); // detected name of the face is been saved  to the 'name'-variable
                }

                NamesPersons[tem - 1] = eyename;
                NamesPersons.Add("");
                label10.Text = StoreEyes[0].Length.ToString();
            }
            tem = 0;
            for (int ee = 0; ee < StoreEyes[0].Length; ee++)
            {
                eyenames = eyenames + NamesPersons[ee];
            }

            label10.Text = eyenames;
            eyenames = "";
            NamesPersons.Clear();


            imageBox1.Image = currentFrame;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            try
            {

                ContTrain = ContTrain + 1;

                //take the 320x240 picture from the cemera and make it 20x20 for TrainedFace
                gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                //TestImageBox.Image = gray;
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                face,
                1.2,
                10,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new Size(20, 20)); // new size of pic only face


                foreach (MCvAvgComp f in facesDetected[0])
                {
                    TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>(); //converting the pic to gray and save it to TranedFace
                    break;
                }

                //Trained image is been save to "Trainedface" variable
                TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainingImages.Add(TrainedFace);
                //adding text box train name to label
                labels.Add(textBox1.Text);


                imageBox2.Image = TrainedFace;


                File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");


                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
                }

                MessageBox.Show(textBox1.Text + "´s face detected and added :)", "Training OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Enable the face detection first", "Training Fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }//////////// face detection /////////////

        private void button3_Click(object sender, EventArgs e)
        {

            grab = new Capture();
            grab.QueryFrame();

            Application.Idle += new EventHandler(framegrab);
            button3.Enabled = false; //camera start button

        }//////////// face detection /////////////

        public void framegrab(object sender, EventArgs e)
        {


            NamePersons.Add("");
            // capture a frame form  device both face and all things on the image
            currentframe = grab.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            gray = currentframe.Convert<Gray, Byte>();

            //(TestImageBox.Image = currentFrame);

            //Result of haarCascade will be on the "MCvAvgComp"-facedetected (is it face or not )
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            MCvAvgComp[][] StoreEyes = gray.DetectHaarCascade(eye, 1.2, 2, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(1, 1));//eye
            MCvAvgComp[][] Mouthdetection = gray.DetectHaarCascade(mouth, 3, 3, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(1, 1));



            foreach (MCvAvgComp eyes in StoreEyes[0]) // more than one eye may be detected so for each eye there will be a rectangle
            {

                results = currentframe.Copy(eyes.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                currentframe.Draw(eyes.rect, new Bgr(Color.Red), 2);
                // for each detecte eye in the image a rectangle will be drawn on the image
                int eyeLableX = eyes.rect.X; // find the X top-left corner of the eye for labeling
                int eyeLableY = eyes.rect.Y; // find the Y top-left corneer of the eye for lableing 
                Point p = new Point(eyeLableX, eyeLableY); // create a point p to attach the lable "EYE" to it
                //currentFrame.Draw(name, ref font, p, new Bgr(Color.Red)); // add a string "EYE" to the image captured from webcam just to be distingushable

            }

            foreach (MCvAvgComp mouths in Mouthdetection[0]) // loop over all the detected MOUTHs
            {
                //currentframe.Draw(mouths.rect, new Bgr(Color.Blue), 2);
                // draw a recangle for each mouth and put it on the display
                int mouthLableX = mouths.rect.X; // find the X top-left corner of the mouth for labling
                int mouthLableY = mouths.rect.Y; // find the Y top-left corner of the mouth for labling
                Point p = new Point(mouthLableX, mouthLableY); // create a point to attach the "MOUTH" lable to it
            }


            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentframe.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                currentframe.Draw(f.rect, new Bgr(Color.Green), 2); //Frame detect colour is 'read'


                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);


                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                       3000,
                       ref termCrit);

                    n = recognizer.Recognize(result); // detected name of the face is been saved  to the 'name'-variable

                    if (n == null)
                    {
                        currentframe.Draw(f.rect, new Bgr(Color.Green), 2); //Frame detect colour is 'read'
                    }
                    //the colour of  the face label name 


                }

                NamePersons[t - 1] = n;
                NamePersons.Add("");

            }
            t = 0;

            for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
            {
                ns = ns + NamePersons[nnn];
            }



            imageBox3.Image = currentframe;
            label6.Text = ns;
            ns = "";

            NamePersons.Clear();

        }

        private void label6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OleDbConnection con = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:/Users/PC/Documents/visual studio 2010/Projects/WindowsFormsApplication23/WindowsFormsApplication23/attendace.accdb");
                con.Open();
                OleDbCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Name FROM tbl_student where StudentID ='" + label6.Text + "'";
                cmd.ExecuteNonQuery();
                OleDbDataAdapter fill2 = new OleDbDataAdapter(cmd);
                fill2.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {

                    label8.Text = "REGISTERED";

                }
                else
                {
                    label8.Text = "NOT REGISTERED";
                }
            }
            catch (Exception xe)
            {
                MessageBox.Show(" " + xe);
            }
        }



    }
}
