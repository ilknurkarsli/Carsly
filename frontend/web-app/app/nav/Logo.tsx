"use client";
import { useParamsStore } from "@/hooks/useParamsStore";
import { GiPoliceCar } from "react-icons/gi";

export default function Logo() {

    const reset = useParamsStore(state=>state.reset);
  return (
    <div onClick={reset} className="cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-500">
      <GiPoliceCar size={34} />
      <div>Carsly Auctions</div>
    </div>
  );
}
