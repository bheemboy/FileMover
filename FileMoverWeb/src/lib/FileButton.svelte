<script>
    export let Type;
    export let busy;
</script>

<button disabled='{busy}' on:click>
    {#if ('DELETE' == Type)}
    <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#384350" stroke-miterlimit="10" d="M2 4.5h20M10.5 8v10M13.5 8v10M7.43 21.5L5 4.5h13.52l-1.97 17H7.43z" style="stroke: #ffffff;"/><path fill="none" stroke="#384350" stroke-miterlimit="10" stroke-width="2" d="M9 3h6" style="stroke: #ffffff;"/></svg>

    {:else if  ('COPYTORIGHT' == Type)}
    <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#485664" stroke-miterlimit="10" d="M12 18l6-6-6-6M6 18l6-6-6-6" style="stroke: #ffffff;"/></svg>
    
    {:else}
    <svg class="svg-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="none" stroke="#485664" stroke-miterlimit="10" d="M12 18l-6-6 6-6M18 18l-6-6 6-6" style="stroke: #ffffff;"/></svg>

    {/if}
</button>

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

</style>
