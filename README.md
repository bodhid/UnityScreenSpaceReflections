# UnityScreenSpaceReflections
SSR - Screen Space Reflections post processing effect

Just smack the script on your Unity Camera

-Calculates per-pixel world position

-Reflects the view direction over the G-Buffer normal and raytraces through the world position texture (64 samples, can be less). The tracing is noisy so there are no visible lines/layers

-Blurs and combines the result


Blur amount and downsampling can be adjusted.

![alt text](https://s17.postimg.cc/qtz1ro067/ssr.jpg)
