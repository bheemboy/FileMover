<script>
    export let Type;
    export let busy;
    export let disable_button = false;

    $: disabled = busy || disable_button;
</script>

<div class="tooltip">

    <button disabled='{disabled}' on:click>
        {#if ('DELETE' == Type)}
            <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#384350" stroke-miterlimit="10" d="M2 4.5h20M10.5 8v10M13.5 8v10M7.43 21.5L5 4.5h13.52l-1.97 17H7.43z" style="stroke: crimson;"/><path fill="none" stroke="#384350" stroke-miterlimit="10" stroke-width="2" d="M9 3h6" style="stroke: crimson;"/></svg>
        {:else if  ('COPYTORIGHT' == Type)}
            <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#485664" stroke-miterlimit="10" d="M12 18l6-6-6-6M6 18l6-6-6-6" style="stroke: #ffffff;"/></svg>
        {:else if  ('COPYTOLEFT' == Type)}
            <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#485664" stroke-miterlimit="10" d="M12 18l-6-6 6-6M18 18l-6-6 6-6" style="stroke: #ffffff;"/></svg>
        {:else if  ('CREATE-SHA256' == Type)}
            <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#384350" stroke-linejoin="round" stroke-width="1.25" d="M7.5 21.5l3-9-4.83.01L7.5 2.5h8l-3.48 7.02H18L7.5 21.5z" style="stroke: lightseagreen;"/></svg>
        {:else if  ('VERIFY-SHA256' == Type)}
            <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#384350" stroke-linecap="round" stroke-miterlimit="10" d="M9 12.73l1.85 1.77L15 10" style="stroke: greenyellow;"/><path fill="none" stroke="#384350" stroke-miterlimit="10" d="M12 22.48C9.8 21.91 4.5 19.9 4.5 17V4.42C5.84 4.16 10 3.27 12 1.67c2 1.6 6.16 2.49 7.5 2.75V17c0 2.9-5.3 4.91-7.5 5.48z" style="stroke: lightseagreen;"/></svg>
        {/if}
    </button>

    {#if ('DELETE' == Type)}
        <span class="tooltiptext">Delete item</span>
    {:else if  ('COPYTORIGHT' == Type)}
        <span class="tooltiptext">Copy item to right</span>
    {:else if  ('COPYTOLEFT' == Type)}
        <span class="tooltiptext">Copy item to left</span>
    {:else if  ('CREATE-SHA256' == Type)}
        <span class="tooltiptext">Create checksum</span>
    {:else if  ('VERIFY-SHA256' == Type)}
        <span class="tooltiptext">Verify checksum</span>
    {/if}

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
    font-size: small;
    width: 120px;
    background-color: rgba(255, 230, 0, 0.753);
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
    transition: opacity 0.2s ease-in, visibility 0s linear 1.5s;
}

/* Show the tooltip text when hovering */
.tooltip:hover .tooltiptext {
    visibility: visible;
    opacity: 1;
    transition: opacity 0.3s ease-in, visibility 0s linear;
    transition-delay: 1s;
}

</style>
