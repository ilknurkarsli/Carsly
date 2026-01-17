"use client";
import { useParamsStore } from "@/hooks/useParamsStore";
import { usePathname, useRouter } from "next/navigation";
import { GiPoliceCar } from "react-icons/gi";

export default function Logo() {

    const router = useRouter();
    const pathName = usePathname();

    const reset = useParamsStore(state=>state.reset);

    function handleReset(){
      if(pathName !== "/") router.push("/");
      reset();
    }

  return (
    <div onClick={handleReset} className="cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-500">
      <GiPoliceCar size={34} />
      <div className="cursor-pointer">Carsly Auctions</div>
    </div>
  );
}
