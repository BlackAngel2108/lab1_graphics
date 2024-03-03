﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace lab1_graphics
{
    abstract class Filters
    {
        int max(int a,int b){
        if (a >= b)
            return a;
        return b;
        }
        int min(int a, int b)
        {
            if (a <= b)
                return a;
            return b;
        }
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public int Rmax = 0, Gmax = 0, Bmax = 0;
        public int Rmin = 255, Gmin = 255, Bmin = 255;
        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker) {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++) {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourceColor = sourceImage.GetPixel(i, j);

                    Rmax = (int)max(Rmax, sourceColor.R);
                    Gmax = (int)max(Rmax, sourceColor.G);
                    Bmax = (int)max(Rmax, sourceColor.B);

                    Rmin = (int)min(Rmin, sourceColor.R);
                    Gmin = (int)min(Rmin, sourceColor.G);
                    Bmin = (int)min(Rmin, sourceColor.B);
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
                
            }
            return resultImage;
        }

        public int Clamp(int value, int min, int max) {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
    class InvertFilter : Filters {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y) {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    };
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double intensity = 0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B;
            Color resultColor = Color.FromArgb((int)(intensity), (int)intensity, (int)intensity);
            return resultColor;
        }
    };
    class PerfectReflectorFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double resultR = 255 * sourceColor.R / Rmax;
            double resultG = 255 * sourceColor.G / Gmax;
            double resultB = 255 * sourceColor.B / Bmax;
            Color resultColor = Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255));
            return resultColor;
        }
    };
    class LinearStretchingFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int R = (Rmax - Rmin);
            int G = (Gmax - Gmin);
            int B = (Bmax - Bmin);
            if (R == 0)
                R = 1;
            if (G == 0)
                G = 1;
            if (B == 0)
                B = 1;
            double resultR = 255 * (sourceColor.R-Rmin) / R;
            double resultG = 255 * (sourceColor.G-Gmin) / G;
            double resultB = 255 * (sourceColor.B-Bmin) / B;
            Color resultColor = Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255));
            return resultColor;
        }
    };

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double intensity = 0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B;
            double k = 15;
            double resultR = intensity + 2 * k;
            double resultG = intensity + 0.5 * k;
            double resultB = intensity - 1 * k;
            Color resultColor = Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255));
            return resultColor;
        }
    };
    class MatrixFilter: Filters{
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel){
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <=radiusY; l++)
                for (int k = -radiusX; k<= radiusX; k ++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
            );
        }
    };
    class BlurFilter: MatrixFilter
    {
        public BlurFilter(){
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    };
    class SharpenFilter : MatrixFilter //резкость
    {
        public SharpenFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = -1;
            kernel[1, 1]=9;
        }
    };
    class GaussianFilter : MatrixFilter{
        public GaussianFilter(){
            createGaussianKernel(3, 2);
        }
        public void createGaussianKernel(int radius, float sigma){
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for(int j= - radius;j<=radius;j++){
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2*sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i=0;i<size;i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
        
    };
    

}
