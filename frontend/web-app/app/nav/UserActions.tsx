"use client";
import { useParamsStore } from "@/hooks/useParamsStore";
import { Dropdown, DropdownDivider, DropdownItem } from "flowbite-react";
import { User } from "next-auth";
import { signOut } from "next-auth/react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useRouter } from "next/navigation";
import { AiFillCar, AiFillTrophy, AiOutlineLogout } from "react-icons/ai";
import { HiCog, HiUser } from "react-icons/hi";

type Props ={
  user: User
}

export default function UserActions({user}: Props) {

  const router = useRouter();
  const patheName = usePathname();

  const setParams = useParamsStore(state=> state.setParams);

  function setWinner(){
    setParams({winner: user.name!, seller: undefined});
    if (patheName !== '/') router.push('/');
  }

  function setSeller (){
    setParams({seller: user.name!, winner: undefined});
    if (patheName !== '/') router.push('/');
  }

  return (
    <Dropdown inline label={`welcome, ${user.name}`} className="cursor-pointer">
      <DropdownItem icon={HiUser} onClick={setSeller}>
        My Auctions
      </DropdownItem>
      <DropdownItem icon={AiFillTrophy} onClick={setWinner}>
        Auction Won
      </DropdownItem>
      <DropdownItem icon={AiFillCar}>
        <Link href="/auctions/create">
          Sell My Car 
        </Link>
      </DropdownItem>
      <DropdownItem icon={HiCog}>
        <Link href="/session">
          Session (dev Only!)
        </Link>
      </DropdownItem>
      <DropdownDivider/>
      <DropdownItem icon={AiOutlineLogout} onClick={()=>signOut({redirectTo:"/"})}>
        SigntOut
      </DropdownItem>
    </Dropdown>
  )
}
