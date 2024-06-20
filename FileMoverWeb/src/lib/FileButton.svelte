<script>
    export let Type;
    export let busy;
</script>

<div class="tooltip">
    <button disabled='{busy}' on:click>
        {#if ('DELETE' == Type)}
        <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#384350" stroke-miterlimit="10" d="M2 4.5h20M10.5 8v10M13.5 8v10M7.43 21.5L5 4.5h13.52l-1.97 17H7.43z" style="stroke: #ffffff;"/><path fill="none" stroke="#384350" stroke-miterlimit="10" stroke-width="2" d="M9 3h6" style="stroke: #ffffff;"/></svg>
        <span class="tooltiptext">Delete item</span>
    
        {:else if  ('COPYTORIGHT' == Type)}
        <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#485664" stroke-miterlimit="10" d="M12 18l6-6-6-6M6 18l6-6-6-6" style="stroke: #ffffff;"/></svg>
        <span class="tooltiptext">Copy item to right</span>
        
        {:else}
        <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#485664" stroke-miterlimit="10" d="M12 18l-6-6 6-6M18 18l-6-6 6-6" style="stroke: #ffffff;"/></svg>
        <span class="tooltiptext">Copy item to left</span>
    
        {/if}
    </button>
</div>

<style>
button {
    background-color: #404040; /* Green background */
    border: 1px solid #404040; /* Remove borders */
    padding: 11px 15px 9px;
    margin-top: 3px;
    cursor: pointer; /* Add a pointer on hover */
    border-radius: 5px; /* Rounded corners */
}

button:disabled,
button:disabled:hover
{
    background-color: #404040; /* Green background */
    border: 1px solid #404040; /* Remove borders */
    opacity: 25%;
    cursor: default;
}

button:hover {
    background-color: #606060;
    border: 1px solid lightgray; /* Remove borders */
}

.svg-icon {
    width: 24px;
    height: 24px;
}

/* Basic styles for the tooltip container */
.tooltip {
    position: relative;
    display: inline-block;
    cursor: pointer;
}

/* Hidden by default, the tooltip text */
.tooltip .tooltiptext {
    visibility: hidden;
    width: 120px;
    background-color: rgb(255, 174, 0);
    color: black;
    text-align: center;
    border-radius: 5px;
    padding: 2px 0;
    
    /* Positioning the tooltip text */
    position: absolute;
    z-index: 1;
    bottom: 100%; /* Position above the element */
    left: 60%;
    margin-left: -60px; /* Center the tooltip */
    
    /* Fade in tooltip */
    opacity: 0;
    transition: opacity 0.3s ease-in-out, visibility 0s linear 1.5s;
}

/* Show the tooltip text when hovering */
.tooltip:hover .tooltiptext {
    visibility: visible;
    opacity: 0.7;
    transition: opacity 0.3s ease-in, visibility 0s linear;
    transition-delay: 1s;
}

</style>
