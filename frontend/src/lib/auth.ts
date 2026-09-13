"use client";

import {
  signInWithEmailAndPassword,
  signOut as firebaseSignOut,
  onAuthStateChanged,
  User,
} from "firebase/auth";
import { getClientAuth } from "./firebase";

export async function signIn(email: string, password: string) {
  const auth = getClientAuth();
  return signInWithEmailAndPassword(auth, email, password);
}

export async function signOut() {
  const auth = getClientAuth();
  return firebaseSignOut(auth);
}

export function onAuthStateChange(callback: (user: User | null) => void) {
  const auth = getClientAuth();
  return onAuthStateChanged(auth, callback);
}

export async function getIdToken(): Promise<string | null> {
  const auth = getClientAuth();
  const user = auth.currentUser;
  if (!user) return null;
  return user.getIdToken();
}
