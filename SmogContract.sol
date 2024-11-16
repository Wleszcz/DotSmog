// SPDX-License-Identifier: MIT
pragma solidity ^0.8.26;

interface ERC20Interface {
    function totalSupply() external view returns (uint);
    function balanceOf(address account) external view returns (uint balance);
    function allowance(address owner, address spender) external view returns (uint remaining);
    function transfer(address recipient, uint amount) external returns (bool success);
    function approve(address spender, uint amount) external returns (bool success);
    function transferFrom(address sender, address recipient, uint amount) external returns (bool success);

    event Transfer(address indexed from, address indexed to, uint value);
    event Approval(address indexed owner, address indexed spender, uint value);
}

contract SmogContract is ERC20Interface {
    string public symbol;
    string public name;
    uint public decimals;
    uint public _totalSupply;

    mapping(address => uint) balances;
    mapping(address => mapping(address => uint)) allowed;

        constructor() {
            symbol = "DKW";
            name = "Smog Coin";
            decimals = 6;
            _totalSupply = 1_000_000 * 10 ** decimals;
            balances[0x37C298fE359B4EccFd6d69C4f51Da0Ef9b484Aa4] = _totalSupply;
            emit Transfer(address(0), 0x37C298fE359B4EccFd6d69C4f51Da0Ef9b484Aa4, _totalSupply);
        }

    function totalSupply() public view returns (uint) {
        return _totalSupply;
    }
 
    function balanceOf(address account) public view returns (uint balance) {
        return balances[account];
    }
    
    function allowance(address ownner, address spender) public view returns (uint remaining) {
        return allowed[ownner][spender];
    }

    function transfer(address recipient, uint amount) public returns (bool success) {
        require(balances[msg.sender] >= amount, "Not enough tokens");
        balances[msg.sender] -= amount;
        balances[recipient] += amount;
        emit Transfer(msg.sender, recipient, amount);
        return true;
    }
 
    function approve(address spender, uint amount) public returns (bool success) {
        allowed[msg.sender][spender] = amount;
        emit Approval(msg.sender, spender, amount);
        return true;
    }
 
    function transferFrom(address from, address to, uint amount) public returns (bool success) {
        require(balances[from] >= amount, "Not enough tokens");
        require(allowed[from][msg.sender] >= amount, "Allowance exceeded");
        balances[from] -= amount;
        balances[to] += amount;
        allowed[from][msg.sender] -= amount;
        emit Transfer(from, to, amount);
        return true;
    }
}