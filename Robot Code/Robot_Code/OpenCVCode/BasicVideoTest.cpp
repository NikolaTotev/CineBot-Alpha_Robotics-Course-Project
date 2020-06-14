#include <iostream> // for standard I/O
#include <string>   // for strings
#include <iomanip>  // for controlling float print precision
#include <sstream>

#include <opencv2/opencv.hpp>
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>  // Gaussian Blur
#include <opencv2/videoio.hpp>
#include <opencv2/highgui.hpp>
using namespace cv;
using namespace std;

int main(int argc, char** argv)
{
	cv::namedWindow("Video Test", WINDOW_AUTOSIZE);
	VideoCapture cap(0);
	if (!cap.isOpened())
	{
		cerr << "ERROR: Unable to open the camera" << endl;
		return 0;
	}

	Mat frame;

	while (true)
	{
		cap >> frame;
		imshow("Video Test", frame);
		if (waitKey(33) >= 0)
		{
			break;
		}
	}

	return 0;
}




