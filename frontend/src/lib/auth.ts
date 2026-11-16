"use client";

import {
  createUserWithEmailAndPassword,
  sendEmailVerification,
  sendPasswordResetEmail,
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

export async function signUp(email: string, password: string) {
  const auth = getClientAuth();
  const credential = await createUserWithEmailAndPassword(auth, email, password);
  await sendEmailVerification(credential.user);
  return credential;
}

export async function signOut() {
  const auth = getClientAuth();
  return firebaseSignOut(auth);
}

/**
 * Recuperación de contraseña (RF-001, RNF-028):
 * Firebase envía un email con el enlace para restablecer la contraseña.
 */
export async function resetPassword(email: string) {
  const auth = getClientAuth();
  await sendPasswordResetEmail(auth, email);
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
