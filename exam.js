function attemptCookSpag(waterStartBoiling, addSpag, cookingCompleted, problem){
    
    console.log("Water added successfully");
    console.log("Waiting for water to boil");
    try {

        var elapsed = 0;
        while(!waterStartBoiling(elapsed))
        elapsed++;
        
        addSpag();
        
        throw new Error("the spag is spoiled")
        cookingCompleted();
    } catch(error)
    {
        problem(error);
    }



}

function onWaterStartedBoiling(timeElapse) {
    return timeElapse >= 5;
}


function onSpagTimeToAdd(){
    console.log("throw spag in the water");
    

}

function onCompleteCooking()
{
    console.log("Spag is done successfully.");
}

function onError(error){
    console.error("Problem has occurred while cooking spaghetti: " + error);

}

function requestDataFromApi(getDataForm, proposalForm, problem){

}
function getDataForm(){

}

function showProductForm(){

}

function sendDataToApi(dataForm, apiForm, problem){

}



// Timer