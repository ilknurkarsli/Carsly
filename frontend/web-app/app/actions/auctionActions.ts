"use server" //any functions that are executed on the server side only
import { fetchWrapper } from "@/lib/fetchWrapper"
import { Auction, PagedResult } from "@/types"
import { FieldValues } from "react-hook-form"

export async function getData(query: string) : Promise<PagedResult<Auction>> {
  return fetchWrapper.get(`search${query}`)
}

export async function updateAuctionTest(): Promise<{status: number, message: string}>{
  const data = {
    millage: Math.floor(Math.random()*10000) +1
  }
  return await fetchWrapper.put("auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c", data)
}

export async function createAuction(data: FieldValues){
  return fetchWrapper.post("auctions", data)
}

export async function getDetailedViewData(id: string): Promise<Auction>{
  return fetchWrapper.get(`auctions/${id}`)
}
export async function updateAuction(id: string, data: FieldValues){
  return fetchWrapper.put(`auctions/${id}`, data)
}
export async function deleteAuction(id: string){
  return fetchWrapper.del(`auctions/${id}`)
}
