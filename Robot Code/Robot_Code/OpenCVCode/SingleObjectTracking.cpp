#include <iostream> // for standard I/O
#include <string>   // for strings
#include <iomanip>  // for controlling float print precision
#include <sstream>

#include <opencv2/opencv.hpp>
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>  // Gaussian Blur
#include <opencv2/videoio.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/tracking.hpp>
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

	Ptr<Tracker> tracker = TrackerKCF::create();
	cap.read(frame);
	Rect2d trackingBox = selectROI(frame, true);
	tracker->init(frame, trackingBox);

	while (true)
	{
		cap.read(frame);
		if (tracker->update(frame, trackingBox))
		{
			rectangle(frame, trackingBox, Scalar(0, 106, 255), 3, 8);
		}

		imshow("Video Test", frame);
		if (waitKey(33) >= 0)
		{
			break;
		}
	}

	return 0;
}




