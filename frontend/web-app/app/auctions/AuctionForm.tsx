"use client"
import { Button, Spinner } from 'flowbite-react';
import { usePathname, useRouter } from 'next/navigation';
import { FieldValues, useForm } from 'react-hook-form'
import Input from '../components/Input';
import { useEffect } from 'react';
import DateInput from '../components/DateInput';
import { createAuction } from './AuthActions';
import toast from 'react-hot-toast';
import { Auction } from '@/types';
import { updateAuction } from '../actions/auctionActions';

type Props = {
    auction?: Auction;
}

export default function AuctionForm({auction}: Props) { 

    const router = useRouter();
    const pathname = usePathname();

    const {control, handleSubmit, setFocus, reset, formState: {isSubmitting, isValid, isDirty}} = useForm({mode: "onTouched"});

    //when the component is rendered (loads), if there is an auction prop, set the form values to the auction details and set focus to the make input
    useEffect(()=>{
        if(auction){
            const {make, model, color, mileage, year}=auction;
            reset({make, model, color, mileage, year});
        }
        setFocus("make");
    }, [setFocus, auction, reset]);

    async function onSubmit(data: FieldValues){
        try {
            let id = "";
            let res = "";
            if (pathname==="/auctions/create"){
                const res = await createAuction(data);
                id = res.id;
            }else{
                if(auction){
                    res = await updateAuction(auction.id, data);
                    id= auction.id;
                }
            }
            router.push(`/auctions/details/${id}`);
        } catch (error: any) {
            toast.error(error.status + " - " + error.message);
        }
    }

  return (
    <form className="flex flex-col mt-3" onSubmit={handleSubmit(onSubmit)}>
        <Input name='make' label="Make" control={control} rules={{required:"Make is required"}}/>
        <Input name='model' label="Model" control={control} rules={{required:"Model is required"}}/>
        <Input name='color' label="Color" control={control} rules={{required:"Color is required"}}/>

        <div className='grid grid-cols-2 gap-3'>
            <Input name='year' label="Year" type="number" control={control} rules={{required:"Year is required"}}/>
            <Input name='mileage' label="Mileage" control={control} rules={{required:"Mileage is required"}}/>
        </div>
        {pathname==="/auctions/create" &&
        <>
            <Input name='imageUrl' label="Image URL" control={control} rules={{required:"Image URL is required"}}/>
            <div className='grid grid-cols-2 gap-3'>
                <Input name='reservePrice' label="Reserve Price (enter 0 if no reserve)" type="number" control={control} rules={{required:"Reserve Price is required"}}/>
                <DateInput
                    name='auctionEnd' 
                    label="Auction End Date/Time" 
                    control={control} 
                    showTimeSelect
                    dateFormat="dd MMMM yyyy h:mm a"
                    rules={{required:"Auction End Date is required"}}/>
            </div>
        </>}
        <div className='flex justify-between'>
            <Button color="alternative" onClick={()=>router.push("/")}>
                Cancel
            </Button>
            <Button outline color="green" type='submit' disabled={!isValid || !isDirty}>
                {isSubmitting && <Spinner size='sm'/>}
                Submit
            </Button>
        </div>
    </form>
  )
}
