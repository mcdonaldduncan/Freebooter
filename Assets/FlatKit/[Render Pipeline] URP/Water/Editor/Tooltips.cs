using System;
using System.Collections.Generic;

namespace FlatKit.Water {
public static class Tooltips {
    public static Dictionary<String, String> map = new Dictionary<string, string> {
	    // Colors
        {"Shallow", "Color at the top of the water."},
		{"Deep", "Color below the surface."},
		{"Shallow depth", "The distance from surface where the transition from shallow to deep color starts."},
		{"Gradient size", "The height of the transition between the shallow and the deep colors."},
		{"Transparency", "How clear the color of the water is. " +
		                 "The transparency doesn't affect other parameters like foam or refractions. " +
		                 "This allows you to achieve awesome weird optical effects."},
		{"Shadow Strength", "How visible the shadow is."},

		// Crest
		{"Color{Crest}", "The color of the wave. It helps accentuate individual waves."},
		{"Size{Crest}", "How much of the wave is colored."},
		{"Sharp transition{Crest}", "How smoothly the accentuated wave blends into overall color of the water."},

		// Wave geometry
		{"Shape{Wave}", "The formula that determine how the waves are shaped and distributed across the mesh. " +
		          "Round is for concentric round-shaped ripples; Grid is more linear movement; " +
		          "Pointy is for more pronounced individual wave peaks."},
		{"Speed{Wave}", "How fast it moves along the `Direction` parameter."},
		{"Amplitude{Wave}", "Sets deviation amount, or, how high it is."},
		{"Frequency{Wave}", "Density of the effect."},
		{"Direction{Wave}", "Direction of the motion."},
		{"Noise{Wave}", "Adds randomness to the wave shape. Use it to add complexity to the surface."},

		// Foam
		{"Source{Foam}", "How the foam is being made — from texture or generated from noise."},
		{"Texture{Foam}", "A single channel texture map of foam."},
		{"Color{Foam}", "Color value. Can be opaque or transparent."},
		{"Shore Depth{Foam}", "Controls the amount of foam in regions where water intersects other scene objects."},
		{"Amount{Foam}", "How often 'grains' occur."},
		{"Sharpness{Foam}", "How smooth or sharp the foam is."},
		{"Scale{Foam}", "How big the foam 'chunks' are."},
		{"Stretch X{Foam}", "How stretched the foam is along X axis."},
		{"Stretch Y{Foam}", "How stretched the foam is along Y axis."},
		
		// Refraction
		
		// Specular
		{"Power", "Makes specular thin or thick. 'Power' value is a multiplier of 'Strength' parameter."},
		{"Strength", "How prominent the specular is."},
    };
}
}