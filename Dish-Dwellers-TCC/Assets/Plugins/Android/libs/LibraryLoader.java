package com.epicgames.mobile.eossdk;

public class LibraryLoader {
    public static void load() {
        try {
            System.loadLibrary("EOSSDK");
        } catch (UnsatisfiedLinkError e) {
            e.printStackTrace();
        }
    }
}
