"use server"

import { auth } from "@/auth";
import { fetchWrapper } from "@/lib/fetchWrapper";
import { FieldValues } from "react-hook-form";
// import { unstable_noStore as noStore } from "next/cache";

export async function getCurrentUser(){
    try {
        // noStore();
       const session = await auth();
       if(!session) return null;
       return session.user;

    } catch (error) {
        console.log("Error fetching current user:", error);
        return null;
    }
}

export async function createAuction (data: FieldValues){
    return fetchWrapper.post("auctions", data);
}