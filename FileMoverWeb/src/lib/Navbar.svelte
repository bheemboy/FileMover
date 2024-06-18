<script>
    export let BackendService;
    export let PathSettings;
    export let SelectedTab;

    BackendService.on("Connect", onConnected);

    async function onConnected() {
        PathSettings = await BackendService.getApplicationFolders();
        if ((PathSettings.length > 0) && SelectedTab < 0) {
            let lastTab = localStorage.getItem("SelectedTab")?? -1;
            lastTab = Number(lastTab)
            if ((lastTab >= PathSettings.length) || (lastTab < 0)) {
                lastTab = 0;
            }
            onTabClicked(lastTab);
        }
    }

    function onTabClicked(i) {
        SelectedTab = i;
        localStorage.setItem("SelectedTab", String(SelectedTab));
    }

</script>

<nav>
    <ul>
        {#if PathSettings.length > 0}
            {#each PathSettings as pathSetting, i}
            <li><a href="#{pathSetting.ID}" class="{i==SelectedTab?'active':''}" on:click={e => onTabClicked(i)}>{pathSetting.Title}</a></li>
            {/each}
        {/if}
        <li style="float: right;" id="logout"><a href="#logout" on:click={e => BackendService.logout()}>Logout</a></li>
    </ul>
</nav>

<style>
    nav {
        width: 100%;
        background-color: #282828;
        display: flex;
        justify-content: space-between;
        align-items: center;
        box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    ul {
        list-style-type: none;
        margin: 0;
        padding: 0;
        overflow: hidden;
        background-color: #282828;
        display: flex;
        justify-content: flex-end;
        min-width: 100%;
    }

    li {
        float: left;
    }

    li a {
        display: block;
        color: lightgray;
        text-align: center;
        padding: 14px 16px;
        text-decoration: none;
    }

    #logout {
        margin-left: auto;
    }

    li a:hover:not(.active) {
        background-color: #353535;
    }

    .active {
        background-color: #555555;
    }
</style>
